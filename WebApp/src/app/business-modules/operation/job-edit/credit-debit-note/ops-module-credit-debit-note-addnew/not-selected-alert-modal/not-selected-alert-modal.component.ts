import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-not-selected-alert-modal',
  templateUrl: './not-selected-alert-modal.component.html',
  styleUrls: ['./not-selected-alert-modal.component.scss']
})
export class NotSelectedAlertModalComponent extends PopupBase implements OnInit {

  constructor() {
    super();
  }

  ngOnInit() {
  }
  close() {
    this.hide();
  }
}
