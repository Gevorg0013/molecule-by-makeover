const TELEGRAM_LIMIT = 3500

export function chunkMessage(text: string): string[] {
  if (text.length <= TELEGRAM_LIMIT) return [text]

  const chunks: string[] = []
  let remaining = text
  while (remaining.length > TELEGRAM_LIMIT) {
    let splitAt = remaining.lastIndexOf('\n', TELEGRAM_LIMIT)
    if (splitAt < TELEGRAM_LIMIT * 0.5) splitAt = TELEGRAM_LIMIT
    chunks.push(remaining.slice(0, splitAt))
    remaining = remaining.slice(splitAt).trimStart()
  }
  if (remaining) chunks.push(remaining)
  return chunks
}

const TOOL_ICONS: Record<string, string> = {
  Bash: '🖥️',
  Read: '📖',
  Write: '📝',
  Edit: '✏️',
  Grep: '🔎',
  Glob: '🔎',
  TodoWrite: '📋',
  WebSearch: '🌐',
  WebFetch: '🌐',
}

export function formatToolUse(toolName: string, input: Record<string, unknown>): string {
  const icon = TOOL_ICONS[toolName] ?? '🔧'
  if (toolName === 'Bash') {
    const cmd = typeof input.command === 'string' ? input.command : ''
    return `${icon} ${truncate(cmd, 200)}`
  }
  const filePath = typeof input.file_path === 'string' ? input.file_path : typeof input.path === 'string' ? input.path : undefined
  if (filePath) return `${icon} ${toolName}: ${filePath}`
  return `${icon} ${toolName}`
}

function truncate(text: string, max: number): string {
  return text.length > max ? `${text.slice(0, max)}…` : text
}
