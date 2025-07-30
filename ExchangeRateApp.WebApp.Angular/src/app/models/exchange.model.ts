import { Currency } from './currency.model'
export interface ExchangeRequest {
  apiName: string;
  from: string;
  to: string;
  dateFrom: string;
  dateTo: string;
}

export interface ExchangeRate {
  date: string;
  value: number;
}

export interface ExchangeResult {
  from: Currency;
  to: Currency;
  rates: ExchangeRate[];
  min: number;
  max: number;
  avg: number;
}
