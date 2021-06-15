import { format } from 'date-fns'

export default (date: Date): string => {
  return format(new Date(date), 'eeee, dd MMMM')
}
