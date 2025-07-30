import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Currency } from '../models/currency.model'
import { ExchangeRequest, ExchangeResult } from '../models/exchange.model'

@Injectable({
  providedIn: 'root'
})
export class CurrencyExchangeService {
  private baseUrl = '/api/ExchangeRate'; 

  constructor(private http: HttpClient) { }

  getApis(): Observable<string[]> {
    return this.http.get<string[]>(`${this.baseUrl}/apis`);
  }

  getCurrencies(apiName: string): Observable<Currency[]> {
    const params = new HttpParams().set('apiName', apiName);
    return this.http.get<Currency[]>(`${this.baseUrl}/currencies`, { params });
  }

  calculateExchange(request: ExchangeRequest): Observable<ExchangeResult> {
    return this.http.post<ExchangeResult>(`${this.baseUrl}/rates`, request);
  }
}
