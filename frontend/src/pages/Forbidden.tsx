import { useTranslation } from 'react-i18next'
import { Link } from 'react-router-dom'

import { Button } from '@/components/ui/button'

export function ForbiddenPage() {
  const { t } = useTranslation()
  return (
    <div className="mx-auto flex max-w-md flex-col items-center gap-4 px-4 py-32 text-center">
      <h1 className="font-serif text-3xl">{t('errors.forbiddenTitle')}</h1>
      <p className="text-muted-foreground">{t('errors.forbiddenSubtitle')}</p>
      <Button asChild>
        <Link to="/">{t('errors.goHome')}</Link>
      </Button>
    </div>
  )
}
