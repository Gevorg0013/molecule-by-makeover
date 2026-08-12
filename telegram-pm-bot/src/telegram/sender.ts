import type { TelegramClient } from 'telegram'
import { config } from '../config/env.js'
import { chunkMessage } from '../utils/telegramFormat.js'
import { logger } from '../utils/logger.js'

let client: TelegramClient | undefined

export function bindClient(c: TelegramClient): void {
  client = c
}

/** 'me' targets Saved Messages; a numeric ALLOWED_CHAT_ID override targets that chat directly. */
function target(): string | number {
  return config.allowedChatId === 'me' ? 'me' : Number(config.allowedChatId)
}

export async function send(text: string): Promise<void> {
  if (!client) throw new Error('Telegram client not bound yet')
  for (const chunk of chunkMessage(text)) {
    try {
      await client.sendMessage(target(), { message: chunk })
    } catch (err) {
      logger.error('Failed to send Telegram message', err)
    }
  }
}
