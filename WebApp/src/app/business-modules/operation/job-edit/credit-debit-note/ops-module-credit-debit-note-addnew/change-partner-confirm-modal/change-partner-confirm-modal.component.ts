import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-change-partner-confirm-modal',
  templateUrl: './change-partner-confirm-modal.component.html',
  styleUrls: ['./change-partner-confirm-modal.component.scss']
})
export class ChangePartnerConfirmModalComponent extends PopupBase implements OnInit {
  @Output() confirmPartner = new EventEmitter<boolean>();
  constructor() {
    super();
  }

  ngOnInit() {
  }
  close() {
    this.hide();
  }
  confirm(isAccept: boolean = false) {
    this.confirmPartner.emit(isAccept);
    this.hide();
  }
}
