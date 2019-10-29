import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-add-country',
  templateUrl: './add-country.component.html'
})
export class AddCountryComponent extends PopupBase implements OnInit {

  constructor() {
    super();
  }

  ngOnInit() {
  }

}
