import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-confirm-delete-job-popup',
  templateUrl: './confirm-delete-job-popup.component.html'
})
export class ConfirmDeleteJobPopupComponent extends PopupBase implements OnInit {
  @Input() jobNo: string;
  @Output() onSubmit: EventEmitter<any> = new EventEmitter<any>();
  constructor() {
    super();
  }

  ngOnInit() {
  }
  deleteJob() {
    this.onSubmit.emit(true);
  }
  onCancel() {
    this.hide();
  }
}
