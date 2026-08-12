import type { TelegramClient } from 'telegram'
import { NewMessage, type NewMessageEvent } from 'telegram/events/index.js'
import { config } from '../config/env.js'
import { dispatch } from '../commands/index.js'
import { logger } from '../utils/logger.js'

export async function startListening(client: TelegramClient): Promise<void> {
  let selfId: string | undefined
  if (config.allowedChatId === 'me') {
    const me = await client.getMe()
    selfId = me.id.toString()
  }

  client.addEventHandler(async (event: NewMessageEvent) => {
    try {
      const message = event.message
      const chatId = message.chatId?.toString()
      const allowed = config.allowedChatId === 'me' ? chatId === selfId : chatId === config.allowedChatId
      if (!allowed) return

      const text = message.text?.trim()
      if (!text) return

      await dispatch(text)
    } catch (err) {
      logger.error('Error handling incoming Telegram message', err)
    }
  }, new NewMessage({}))

  logger.info(`Listening on ${config.allowedChatId === 'me' ? 'Saved Messages (your own chat)' : `chat ${config.allowedChatId}`}`)
}
