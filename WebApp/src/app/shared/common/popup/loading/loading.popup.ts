import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';
import { eFMSPopup } from '../popup';

@Component({
  selector: 'loading-popup',
  templateUrl: './loading.popup.html',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class LoadingPopupComponent extends eFMSPopup {
  isCompleted: boolean = false;
  isSuccessDownload: boolean = false;
  body: string = '';
  constructor() { 
    super();
  }

  ngOnInit() {
    this.body = "Report is in running process! <br><b>Please wait while downloading...</b>";
  }

  close() {
    this.resetPopup();
    this.hide();
  }

  downloadSuccess(){
    this.isCompleted = true;
    this.isSuccessDownload = true;
  }

  downloadFail(){
    this.isCompleted = true;
    this.isSuccessDownload = false;
  }

  resetPopup(){
    this.isCompleted = false;
    this.isSuccessDownload = false;
  }
}
