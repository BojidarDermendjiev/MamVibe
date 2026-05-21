import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import MockAdapter from 'axios-mock-adapter'

// Each test re-imports axiosClient with fresh mocks via vi.resetModules()
describe('axiosClient interceptors', () => {
  beforeEach(() => {
    vi.resetModules()
    localStorage.clear()
  })

  afterEach(() => {
    vi.restoreAllMocks()
    localStorage.clear()
  })

  async function loadClient(accessToken: string | null = null) {
    const logout = vi.fn()
    vi.doMock('../store/authStore', () => ({
      useAuthStore: {
        getState: vi.fn().mockReturnValue({ accessToken, logout }),
        setState: vi.fn(),
      },
    }))
    const axiosMod = await import('axios')
    const mod = await import('./axiosClient')
    return { client: mod.default, logout, axios: axiosMod.default }
  }

  it('adds Bearer token from authStore when accessToken is set', async () => {
    const { client } = await loadClient('tok-abc')
    const mock = new MockAdapter(client)
    let capturedAuth: string | undefined
    mock.onGet('/ping').reply((config) => {
      capturedAuth = config.headers?.Authorization as string
      return [200, {}]
    })
    await client.get('/ping')
    expect(capturedAuth).toBe('Bearer tok-abc')
    mock.restore()
  })

  it('omits Authorization header when no accessToken', async () => {
    const { client } = await loadClient(null)
    const mock = new MockAdapter(client)
    let capturedAuth: string | undefined
    mock.onGet('/ping').reply((config) => {
      capturedAuth = config.headers?.Authorization as string
      return [200, {}]
    })
    await client.get('/ping')
    expect(capturedAuth).toBeUndefined()
    mock.restore()
  })

  it('sets X-Language to stored language when valid', async () => {
    localStorage.setItem('language', 'bg')
    const { client } = await loadClient()
    const mock = new MockAdapter(client)
    let lang: string | undefined
    mock.onGet('/ping').reply((config) => {
      lang = config.headers?.['X-Language'] as string
      return [200, {}]
    })
    await client.get('/ping')
    expect(lang).toBe('bg')
    mock.restore()
  })

  it('defaults X-Language to en for unsupported locale', async () => {
    localStorage.setItem('language', 'fr')
    const { client } = await loadClient()
    const mock = new MockAdapter(client)
    let lang: string | undefined
    mock.onGet('/ping').reply((config) => {
      lang = config.headers?.['X-Language'] as string
      return [200, {}]
    })
    await client.get('/ping')
    expect(lang).toBe('en')
    mock.restore()
  })

  it('does not set application/json Content-Type for FormData', async () => {
    const { client } = await loadClient()
    const mock = new MockAdapter(client)
    let ct: string | undefined
    mock.onPost('/upload').reply((config) => {
      ct = config.headers?.['Content-Type'] as string
      return [200, {}]
    })
    const fd = new FormData()
    fd.append('file', new Blob(['data']))
    await client.post('/upload', fd)
    expect(ct).not.toContain('application/json')
    mock.restore()
  })

  it('sanitizes 5xx response body to generic message', async () => {
    const { client } = await loadClient()
    const mock = new MockAdapter(client)
    mock.onGet('/boom').reply(500, { detail: 'Internal details' })
    try {
      await client.get('/boom')
    } catch (err: unknown) {
      const e = err as { response: { data: { error: string } } }
      expect(e.response.data.error).toBe('A server error occurred. Please try again later.')
    }
    mock.restore()
  })

  it('calls logout when refresh endpoint returns 401', async () => {
    const { client, logout } = await loadClient('expired')
    const mock = new MockAdapter(client)
    mock.onPost('/api/auth/refresh').reply(401)
    mock.onGet('/protected').reply(401)
    try {
      await client.get('/protected')
    } catch {
      // expected
    }
    expect(logout).toHaveBeenCalled()
    mock.restore()
  })

  it('retries original request after successful token refresh', async () => {
    const { client, axios: ax } = await loadClient('old-token')
    const mock = new MockAdapter(client)
    // The refresh call goes through bare axios.post (not the client), so spy on it
    const refreshSpy = vi.spyOn(ax, 'post').mockResolvedValueOnce({
      data: { accessToken: 'new-token', user: {} },
    })
    mock.onGet('/protected').replyOnce(401).onGet('/protected').reply(200, { ok: true })
    const res = await client.get('/protected')
    expect(res.data).toEqual({ ok: true })
    refreshSpy.mockRestore()
    mock.restore()
  })

  it('queues concurrent 401 requests and replays them after refresh', async () => {
    const { client, axios: ax } = await loadClient('old-token')
    const mock = new MockAdapter(client)
    const refreshSpy = vi.spyOn(ax, 'post').mockResolvedValueOnce({
      data: { accessToken: 'new-token', user: {} },
    })
    mock.onGet('/a').replyOnce(401).onGet('/a').reply(200, { from: 'a' })
    mock.onGet('/b').replyOnce(401).onGet('/b').reply(200, { from: 'b' })
    const [a, b] = await Promise.all([client.get('/a'), client.get('/b')])
    expect(a.data).toEqual({ from: 'a' })
    expect(b.data).toEqual({ from: 'b' })
    refreshSpy.mockRestore()
    mock.restore()
  })
})
