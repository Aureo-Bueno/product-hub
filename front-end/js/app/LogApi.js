/** API utility for the real-time activity log SSE stream. */
export class LogApi {
  /** Returns the SSE endpoint URL for streaming recent activity logs. @returns {string} */
  streamUrl() {
    return "/api/logs/stream";
  }
}
