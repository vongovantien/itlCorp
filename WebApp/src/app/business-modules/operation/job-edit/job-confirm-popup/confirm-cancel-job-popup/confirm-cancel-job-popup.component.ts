import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-confirm-cancel-job-popup',
  templateUrl: './confirm-cancel-job-popup.component.html'
})
export class ConfirmCancelJobPopupComponent extends PopupBase implements OnInit {

  constructor() {
    super();
  }

  ngOnInit() {
  }
  close() {
    this.hide();
  }
}
