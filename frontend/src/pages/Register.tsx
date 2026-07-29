import { zodResolver } from '@hookform/resolvers/zod'
import { useForm } from 'react-hook-form'
import { useTranslation } from 'react-i18next'
import { Link, useNavigate } from 'react-router-dom'
import { z } from 'zod'

import { Button } from '@/components/ui/button'
import { Input } from '@/components/ui/input'
import { Label } from '@/components/ui/label'
import { authErrorMessage, useRegister } from '@/hooks/useAuth'

const schema = z.object({
  firstName: z.string().min(1).max(100),
  lastName: z.string().min(1).max(100),
  email: z.string().email(),
  password: z
    .string()
    .min(8)
    .regex(/[a-z]/, { message: 'lowercase required' })
    .regex(/[A-Z]/, { message: 'uppercase required' })
    .regex(/\d/, { message: 'digit required' }),
})

type FormValues = z.infer<typeof schema>

export function RegisterPage() {
  const { t } = useTranslation()
  const navigate = useNavigate()
  const register_ = useRegister()
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<FormValues>({ resolver: zodResolver(schema) })

  function onSubmit(values: FormValues) {
    register_.mutate(values, { onSuccess: () => navigate('/', { replace: true }) })
  }

  return (
    <div>
      <h1 className="font-serif text-2xl">{t('auth.registerTitle')}</h1>
      <p className="mt-1 text-sm text-muted-foreground">{t('auth.registerSubtitle')}</p>

      <form onSubmit={handleSubmit(onSubmit)} className="mt-6 flex flex-col gap-4">
        <div className="grid grid-cols-2 gap-4">
          <div>
            <Label className="mb-1.5">{t('auth.firstName')}</Label>
            <Input {...register('firstName')} />
            {errors.firstName && <p className="mt-1 text-xs text-destructive">{errors.firstName.message}</p>}
          </div>
          <div>
            <Label className="mb-1.5">{t('auth.lastName')}</Label>
            <Input {...register('lastName')} />
            {errors.lastName && <p className="mt-1 text-xs text-destructive">{errors.lastName.message}</p>}
          </div>
        </div>
        <div>
          <Label className="mb-1.5">{t('common.email')}</Label>
          <Input type="email" {...register('email')} />
          {errors.email && <p className="mt-1 text-xs text-destructive">{errors.email.message}</p>}
        </div>
        <div>
          <Label className="mb-1.5">{t('auth.password')}</Label>
          <Input type="password" {...register('password')} />
          <p className="mt-1 text-xs text-muted-foreground">{t('auth.passwordHint')}</p>
          {errors.password && <p className="mt-1 text-xs text-destructive">{errors.password.message}</p>}
        </div>
        {register_.isError && <p className="text-sm text-destructive">{authErrorMessage(register_.error)}</p>}
        <Button type="submit" size="lg" disabled={register_.isPending}>
          {t('auth.signUp')}
        </Button>
      </form>

      <p className="mt-6 text-center text-sm text-muted-foreground">
        {t('auth.haveAccount')}{' '}
        <Link to="/login" className="font-medium text-foreground hover:underline">
          {t('auth.signIn')}
        </Link>
      </p>
    </div>
  )
}
