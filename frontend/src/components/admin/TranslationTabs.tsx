import type { ReactNode } from 'react'

import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import { LANGUAGES, type LanguageCode } from '@/types/enums'

export function TranslationTabs({ children }: { children: (lang: LanguageCode) => ReactNode }) {
  return (
    <Tabs defaultValue="en">
      <TabsList>
        {LANGUAGES.map((lang) => (
          <TabsTrigger key={lang} value={lang} className="uppercase">
            {lang}
          </TabsTrigger>
        ))}
      </TabsList>
      {LANGUAGES.map((lang) => (
        <TabsContent key={lang} value={lang} className="pt-4">
          {children(lang)}
        </TabsContent>
      ))}
    </Tabs>
  )
}
