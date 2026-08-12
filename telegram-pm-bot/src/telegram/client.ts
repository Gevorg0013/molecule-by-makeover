import fs from 'node:fs'
import { TelegramClient } from 'telegram'
import { StringSession } from 'telegram/sessions/index.js'
import { config } from '../config/env.js'

export function loadSessionString(): string {
  try {
    return fs.readFileSync(config.sessionFile, 'utf-8').trim()
  } catch {
    return ''
  }
}

export async function createTelegramClient(): Promise<TelegramClient> {
  const sessionString = loadSessionString()
  if (!sessionString) {
    throw new Error(`No Telegram session found at ${config.sessionFile}. Run \`npm run login\` first.`)
  }

  const client = new TelegramClient(new StringSession(sessionString), config.telegramApiId, config.telegramApiHash, {
    connectionRetries: 5,
  })
  await client.connect()

  if (!(await client.isUserAuthorized())) {
    throw new Error('Telegram session is no longer authorized. Run `npm run login` again.')
  }

  return client
}
