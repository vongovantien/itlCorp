import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-cancel-create-job-popup',
  templateUrl: './cancel-create-job-popup.component.html'
})
export class CancelCreateJobPopupComponent extends PopupBase implements OnInit {

  constructor() {
    super();
  }

  ngOnInit() {
  }
  close() {
    this.hide();
  }
}
