const DESTRUCTIVE_PATTERNS: RegExp[] = [
  /\brm\s+(-\w*r\w*f|-\w*f\w*r|-rf|-fr)\b/i,
  /\bRemove-Item\b.*(-Recurse|-Force)/i,
  /\bdel\s+\/[fsq]/i,
  /\bgit\s+push\s+.*(--force|-f)\b/i,
  /\bgit\s+reset\s+--hard\b/i,
  /\bgit\s+clean\s+-[a-z]*d[a-z]*f\b/i,
  /\bgit\s+branch\s+-D\b/,
  /\bDROP\s+TABLE\b/i,
  /\bDROP\s+DATABASE\b/i,
  /\bTRUNCATE\s+TABLE\b/i,
  /\bDELETE\s+FROM\s+\w+\s*(;|$)/i,
  /\bdocker\s+(system\s+prune|volume\s+rm|rm\s+-f)/i,
  /\bkubectl\s+delete\b/i,
  /\bchmod\s+-R\s+777\b/i,
  /\bformat\s+[a-z]:/i,
  /\bshutdown\b/i,
  /\bmkfs\b/i,
]

export function isDestructiveCommand(command: string): boolean {
  return DESTRUCTIVE_PATTERNS.some((pattern) => pattern.test(command))
}
