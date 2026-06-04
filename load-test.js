import http from 'k6/http';
import { sleep, check } from 'k6';

const BASE_URL = 'http://localhost:5038';

export default function () {
  const res = http.get(`${BASE_URL}/api/items?page=1&pageSize=12`);

  check(res, {
    'status 200': (r) => r.status === 200,
    'response < 1s': (r) => r.timings.duration < 1000,
  });

  sleep(1);
}
