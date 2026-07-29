import { type FormEvent, useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import { useNavigate, useParams } from 'react-router-dom'
import { toast } from 'sonner'

import { AdminPageHeader } from '@/components/admin/AdminPageHeader'
import { Button } from '@/components/ui/button'
import { Checkbox } from '@/components/ui/checkbox'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from '@/components/ui/select'
import { useAdminCoupon, useCreateCoupon, useUpdateCoupon } from '@/hooks/admin/useAdminCoupons'
import { DiscountType, discountTypeLabels } from '@/types/enums'
import type { CouponUpsertRequest } from '@/types/dto'

export function AdminCouponEditPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const { id } = useParams()
  const { data: coupon } = useAdminCoupon(id)
  const createCoupon = useCreateCoupon()
  const updateCoupon = useUpdateCoupon()

  const [code, setCode] = useState('')
  const [discountType, setDiscountType] = useState<number>(DiscountType.Percent)
  const [discountValue, setDiscountValue] = useState('0')
  const [minOrderAmount, setMinOrderAmount] = useState('')
  const [maxUses, setMaxUses] = useState('')
  const [startsAt, setStartsAt] = useState('')
  const [expiresAt, setExpiresAt] = useState('')
  const [isActive, setIsActive] = useState(true)

  useEffect(() => {
    if (!coupon) return
    setCode(coupon.code)
    setDiscountType(coupon.discountType)
    setDiscountValue(String(coupon.discountValue))
    setMinOrderAmount(coupon.minOrderAmount != null ? String(coupon.minOrderAmount) : '')
    setMaxUses(coupon.maxUses != null ? String(coupon.maxUses) : '')
    setStartsAt(coupon.startsAt ? coupon.startsAt.slice(0, 10) : '')
    setExpiresAt(coupon.expiresAt ? coupon.expiresAt.slice(0, 10) : '')
    setIsActive(coupon.isActive)
  }, [coupon])

  function handleSubmit(e: FormEvent) {
    e.preventDefault()
    const body: CouponUpsertRequest = {
      code,
      discountType: discountType as DiscountType,
      discountValue: Number(discountValue),
      minOrderAmount: minOrderAmount ? Number(minOrderAmount) : null,
      maxUses: maxUses ? Number(maxUses) : null,
      startsAt: startsAt ? new Date(startsAt).toISOString() : null,
      expiresAt: expiresAt ? new Date(expiresAt).toISOString() : null,
      isActive,
    }
    if (id) {
      updateCoupon.mutate({ id, body }, { onSuccess: () => toast.success(t('common.save')) })
    } else {
      createCoupon.mutate(body, {
        onSuccess: (res) => {
          toast.success(t('common.save'))
          navigate(`/admin/coupons/${res.id}`)
        },
      })
    }
  }

  const isSaving = createCoupon.isPending || updateCoupon.isPending

  return (
    <div>
      <AdminPageHeader title={id ? t('common.edit') : t('admin.newCoupon')} />
      <form onSubmit={handleSubmit} className="flex max-w-lg flex-col gap-4">
        <div>
          <Label className="mb-1.5">{t('admin.code')}</Label>
          <Input value={code} onChange={(e) => setCode(e.target.value.toUpperCase())} required />
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <Label className="mb-1.5">{t('admin.discount')}</Label>
            <Select value={String(discountType)} onValueChange={(v) => setDiscountType(Number(v))}>
              <SelectTrigger className="w-full">
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                {Object.entries(discountTypeLabels)
                  .filter(([v]) => v !== String(DiscountType.None))
                  .map(([value, label]) => (
                    <SelectItem key={value} value={value}>
                      {label}
                    </SelectItem>
                  ))}
              </SelectContent>
            </Select>
          </div>
          <div>
            <Label className="mb-1.5">Value</Label>
            <Input type="number" step="0.01" value={discountValue} onChange={(e) => setDiscountValue(e.target.value)} required />
          </div>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <Label className="mb-1.5">Min Order Amount</Label>
            <Input type="number" value={minOrderAmount} onChange={(e) => setMinOrderAmount(e.target.value)} />
          </div>
          <div>
            <Label className="mb-1.5">Max Uses</Label>
            <Input type="number" value={maxUses} onChange={(e) => setMaxUses(e.target.value)} />
          </div>
        </div>
        <div className="grid grid-cols-2 gap-4">
          <div>
            <Label className="mb-1.5">Starts At</Label>
            <Input type="date" value={startsAt} onChange={(e) => setStartsAt(e.target.value)} />
          </div>
          <div>
            <Label className="mb-1.5">Expires At</Label>
            <Input type="date" value={expiresAt} onChange={(e) => setExpiresAt(e.target.value)} />
          </div>
        </div>
        <label className="flex items-center gap-2 text-sm">
          <Checkbox checked={isActive} onCheckedChange={(v) => setIsActive(!!v)} />
          {t('common.active')}
        </label>
        <Button type="submit" size="lg" disabled={isSaving} className="self-start">
          {isSaving ? t('common.saving') : t('common.save')}
        </Button>
      </form>
    </div>
  )
}
