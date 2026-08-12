import fs from 'node:fs'
import path from 'node:path'
import readline from 'node:readline/promises'
import { TelegramClient } from 'telegram'
import { StringSession } from 'telegram/sessions/index.js'
import { config } from '../config/env.js'

const rl = readline.createInterface({ input: process.stdin, output: process.stdout })

async function ask(question: string): Promise<string> {
  const answer = await rl.question(question)
  return answer.trim()
}

async function main(): Promise<void> {
  const stringSession = new StringSession('')
  const client = new TelegramClient(stringSession, config.telegramApiId, config.telegramApiHash, {
    connectionRetries: 5,
  })

  await client.start({
    phoneNumber: async () => ask('Phone number (with country code, e.g. +15551234567): '),
    password: async (hint) => ask(`2FA password${hint ? ` (hint: ${hint})` : ''}: `),
    phoneCode: async () => ask('Login code sent to your Telegram app: '),
    onError: async (err) => {
      console.error(err)
      return false
    },
  })

  const sessionString = stringSession.save()
  fs.mkdirSync(path.dirname(config.sessionFile), { recursive: true })
  fs.writeFileSync(config.sessionFile, sessionString, { mode: 0o600 })

  const me = await client.getMe()
  console.log(`\nLogged in as ${me.firstName ?? ''}${me.username ? ` (@${me.username})` : ''}.`)
  console.log(`Session saved to ${config.sessionFile}`)
  console.log(`Your Telegram user ID (used by default for the Saved Messages chat): ${me.id.toString()}`)

  rl.close()
  await client.disconnect()
  process.exit(0)
}

main().catch((err) => {
  console.error('Login failed:', err)
  process.exit(1)
})
