import { DatabaseSync } from 'node:sqlite'
import fs from 'node:fs'
import path from 'node:path'
import { config } from '../config/env.js'
import { migrate } from './schema.js'

fs.mkdirSync(path.dirname(config.dbFile), { recursive: true })

export const db = new DatabaseSync(config.dbFile)
db.exec('PRAGMA journal_mode = WAL')
db.exec('PRAGMA foreign_keys = ON')
migrate(db)
