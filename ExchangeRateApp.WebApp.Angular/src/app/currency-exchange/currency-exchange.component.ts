import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { CurrencyExchangeService } from '../services/currency-exchange.service';
import { ExchangeResult, ExchangeRequest, ExchangeRate } from '../models/exchange.model';
import { Currency } from '../models/currency.model';

import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, MAT_DATE_FORMATS } from '@angular/material/core';
import { MatDividerModule } from '@angular/material/divider';


export const DATE_FORMATS = {
  parse: {
    dateInput: 'yyyy-MM-dd',
  },
  display: {
    dateInput: 'yyyy-MM-dd',
    monthYearLabel: 'MMM yyyy',
    dateA11yLabel: 'LL',
    monthYearA11yLabel: 'MMMM yyyy',
  }
};

@Component({
  selector: 'app-currency-exchange',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule,
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    MatButtonModule,
    MatTableModule,
    MatProgressSpinnerModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatDividerModule], 
  templateUrl: './currency-exchange.component.html',
  styleUrls: ['./currency-exchange.component.css'],
  providers: [{ provide: MAT_DATE_FORMATS, useValue: DATE_FORMATS } ]
})

export class CurrencyExchangeComponent implements OnInit {
  apis: string[] = [];
  currencies: Currency[] = [];
  exchangeForm: FormGroup;
  exchangeResult: ExchangeResult | null = null;
  loadingCurrencies = false;
  loadingExchange = false;
  error: string | null = null;
  dataSource = new MatTableDataSource<ExchangeRate>();
  displayedColumns: string[] = ['date', 'value'];

  constructor(private service: CurrencyExchangeService, private fb: FormBuilder) {
    this.exchangeForm = this.fb.group({
      apiName: ['', Validators.required],
      from: ['', Validators.required],
      to: ['', Validators.required],
      dateFrom: ['', Validators.required],
      dateTo: ['', Validators.required]
    });
  }

  ngOnInit(): void {
    this.service.getApis().subscribe({
      next: (apis) => this.apis = apis,
      error: (err) => this.error = 'Failed to load APIs'
    });

    // gdy zmieni się API, pobierz waluty
    this.exchangeForm.get('apiName')?.valueChanges.subscribe(api => {
      this.currencies = [];
      this.exchangeResult = null;
      if (api) {
        this.loadingCurrencies = true;
        this.service.getCurrencies(api).subscribe({
          next: (currencies) => {
            this.currencies = currencies;
            this.loadingCurrencies = false;
          },
          error: () => {
            this.error = 'Failed to load currencies';
            this.loadingCurrencies = false;
          }
        });
      }
    });
  }

  onSubmit(): void {
    if (this.exchangeForm.invalid) return;

    this.error = null;
    this.exchangeResult = null;
    this.loadingExchange = true;

    const request: ExchangeRequest = {
      apiName: this.exchangeForm.value.apiName,
      from: this.exchangeForm.value.from,
      to: this.exchangeForm.value.to,
      dateFrom: this.formatDate(this.exchangeForm.value.dateFrom),
      dateTo: this.formatDate(this.exchangeForm.value.dateTo)
    };


    this.service.calculateExchange(request)
      .subscribe({
        next: (result) => {
          this.exchangeResult = result;
          this.dataSource.data = result.rates
          this.loadingExchange = false;
        },
        error: () => {
          this.error = 'Failed to calculate exchange';
          this.loadingExchange = false;
        }
      });
  }

  formatDate(date: Date): string {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}
