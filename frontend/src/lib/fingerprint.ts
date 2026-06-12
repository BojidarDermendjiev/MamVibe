import FingerprintJS, { type Agent } from "@fingerprintjs/fingerprintjs";

// Lazy-loaded singleton — FingerprintJS warns about creating multiple agents in the
// same browser tab. `load()` is idempotent across re-renders thanks to the cached promise.
let agentPromise: Promise<Agent> | null = null;

function getAgent(): Promise<Agent> {
  if (!agentPromise) agentPromise = FingerprintJS.load();
  return agentPromise;
}

/**
 * Returns the FingerprintJS visitor id for the current browser. The id is stable across
 * sessions on the same device/browser combination but can rotate when the user wipes
 * site data, switches browser profiles, or runs privacy modes. Throws if the agent fails
 * to load — the caller must surface a retry-friendly message to the user.
 */
export async function getVisitorId(): Promise<string> {
  const agent = await getAgent();
  const result = await agent.get();
  return result.visitorId;
}
