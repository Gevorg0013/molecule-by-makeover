import { useState } from 'react'
import { useTranslation } from 'react-i18next'
import { toast } from 'sonner'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Badge } from '@/components/ui/badge'
import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs'
import {
  useAdminLanguages,
  useAdminSettings,
  useCreateLanguage,
  useUpdateLanguage,
  useUpdateSetting,
} from '@/hooks/admin/useAdminSettings'

const GROUPS = ['general', 'seo', 'shipping']

export function AdminSettingsPage() {
  const { t } = useTranslation()

  return (
    <div>
      <AdminPageHeader title={t('admin.settings')} />
      <Tabs defaultValue="general">
        <TabsList>
          {GROUPS.map((g) => (
            <TabsTrigger key={g} value={g} className="capitalize">
              {g}
            </TabsTrigger>
          ))}
          <TabsTrigger value="languages">{t('admin.languages')}</TabsTrigger>
        </TabsList>
        {GROUPS.map((g) => (
          <TabsContent key={g} value={g} className="pt-4">
            <SettingsGroup group={g} />
          </TabsContent>
        ))}
        <TabsContent value="languages" className="pt-4">
          <LanguagesPanel />
        </TabsContent>
      </Tabs>
    </div>
  )
}

function SettingsGroup({ group }: { group: string }) {
  const { t } = useTranslation()
  const { data: settings, isLoading } = useAdminSettings(group)
  const updateSetting = useUpdateSetting(group)
  const [values, setValues] = useState<Record<string, string>>({})
  const [newKey, setNewKey] = useState('')

  if (isLoading) return <p className="text-sm text-muted-foreground">{t('common.loading')}</p>

  return (
    <div className="flex max-w-lg flex-col gap-3">
      {settings?.map((s) => (
        <div key={s.key} className="flex items-end gap-2">
          <div className="flex-1">
            <Label className="mb-1.5">{s.key}</Label>
            <Input
              defaultValue={s.value ?? ''}
              onChange={(e) => setValues((v) => ({ ...v, [s.key]: e.target.value }))}
            />
          </div>
          <Button
            variant="outline"
            disabled={updateSetting.isPending}
            onClick={() =>
              updateSetting.mutate(
                { key: s.key, body: { value: values[s.key] ?? s.value } },
                { onSuccess: () => toast.success(t('common.save')) },
              )
            }
          >
            {t('common.save')}
          </Button>
        </div>
      ))}

      <div className="mt-4 flex items-end gap-2 border-t pt-4">
        <div className="flex-1">
          <Label className="mb-1.5">{t('admin.key')}</Label>
          <Input value={newKey} onChange={(e) => setNewKey(e.target.value)} placeholder="new_setting_key" />
        </div>
        <Button
          variant="outline"
          disabled={!newKey}
          onClick={() =>
            updateSetting.mutate(
              { key: newKey, body: { value: '' } },
              {
                onSuccess: () => {
                  toast.success(t('common.add'))
                  setNewKey('')
                },
              },
            )
          }
        >
          {t('common.add')}
        </Button>
      </div>
    </div>
  )
}

function LanguagesPanel() {
  const { t } = useTranslation()
  const { data: languages, isLoading } = useAdminLanguages()
  const updateLanguage = useUpdateLanguage()
  const createLanguage = useCreateLanguage()
  const [code, setCode] = useState('')
  const [name, setName] = useState('')

  if (isLoading) return <p className="text-sm text-muted-foreground">{t('common.loading')}</p>

  return (
    <div className="flex max-w-lg flex-col gap-3">
      {languages?.map((lang) => (
        <div key={lang.id} className="flex items-center justify-between rounded-md border px-3 py-2">
          <div>
            <span className="font-medium">{lang.name}</span>{' '}
            <span className="text-muted-foreground uppercase">({lang.code})</span>
            {lang.isDefault && (
              <Badge variant="secondary" className="ml-2">
                {t('admin.isDefault')}
              </Badge>
            )}
          </div>
          <Button
            size="sm"
            variant="outline"
            onClick={() =>
              updateLanguage.mutate({
                id: lang.id,
                body: { code: lang.code, name: lang.name, isDefault: lang.isDefault, isActive: !lang.isActive },
              })
            }
          >
            {lang.isActive ? t('common.active') : t('common.inactive')}
          </Button>
        </div>
      ))}

      <div className="mt-4 flex items-end gap-2 border-t pt-4">
        <div>
          <Label className="mb-1.5">{t('admin.code')}</Label>
          <Input value={code} onChange={(e) => setCode(e.target.value)} className="w-20" maxLength={2} />
        </div>
        <div className="flex-1">
          <Label className="mb-1.5">{t('common.name')}</Label>
          <Input value={name} onChange={(e) => setName(e.target.value)} />
        </div>
        <Button
          variant="outline"
          disabled={!code || !name}
          onClick={() =>
            createLanguage.mutate(
              { code, name, isDefault: false, isActive: true },
              {
                onSuccess: () => {
                  toast.success(t('common.add'))
                  setCode('')
                  setName('')
                },
              },
            )
          }
        >
          {t('common.add')}
        </Button>
      </div>
    </div>
  )
}
