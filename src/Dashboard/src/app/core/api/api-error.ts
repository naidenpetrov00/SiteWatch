export interface ApiErrorInit {
  method: string;
  url: string;
  status: number;
  statusText: string;
  body: unknown;
  message?: string;
  cause?: unknown;
}

export class ApiError extends Error {
  override cause?: unknown;
  readonly method: string;
  readonly url: string;
  readonly status: number;
  readonly statusText: string;
  readonly body: unknown;

  constructor(init: ApiErrorInit) {
    super(
      init.message ??
        `${init.method} ${init.url} failed with status ${init.status}`
    );

    this.name = 'ApiError';
    this.method = init.method;
    this.url = init.url;
    this.status = init.status;
    this.statusText = init.statusText;
    this.body = init.body;
    this.cause = init.cause;

    Object.setPrototypeOf(this, ApiError.prototype);
  }
}
