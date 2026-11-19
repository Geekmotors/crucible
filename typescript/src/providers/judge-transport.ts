import { CrucibleError } from '../core/errors.js';
import type { FetchLike } from './provider-boundary.js';

class JudgeTransport {
  private chosen?: FetchLike;
  private sealed = false;

  private static platformFetch: FetchLike = (url, init) => globalThis.fetch(url, init);

  constructor(private readonly fallback: FetchLike = JudgeTransport.platformFetch) {}

  fetchLike: FetchLike = (url, init) => (this.chosen ?? this.fallback)(url, init);

  use(candidate: unknown): void {
    if (typeof candidate !== 'function') {
      throw new CrucibleError('usage', 'crucible.useFetch needs a fetch function.');
    }
    if (candidate === this.chosen) {
      return;
    }
    if (this.sealed) {
      throw new CrucibleError(
        'usage',
        'crucible.useFetch must be called before the first Crucible test starts.',
      );
    }
    if (this.chosen !== undefined) {
      throw new CrucibleError(
        'usage',
        'crucible.useFetch was already called with a different fetch; call it once.',
      );
    }
    this.chosen = candidate as FetchLike;
  }

  seal(): void {
    this.sealed = true;
  }
}

export { JudgeTransport };
