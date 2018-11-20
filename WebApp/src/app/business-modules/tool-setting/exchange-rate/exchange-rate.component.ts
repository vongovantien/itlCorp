import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'app-exchange-rate',
  templateUrl: './exchange-rate.component.html',
  styleUrls: ['./exchange-rate.component.scss']
})
export class ExchangeRateComponent implements OnInit {

  constructor() { }

  ngOnInit() {
  }
   /**
   * ng2-select
   */
  public items: Array<string> = ['USD', 'JPY', 'SGD', 'EUR', 'GBP', 'HKD'];

}
