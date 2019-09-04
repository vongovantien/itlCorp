import { Component, OnInit } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';

@Component({
  selector: 'app-upload-alert',
  templateUrl: './upload-alert.component.html'
})
export class UploadAlertComponent extends PopupBase implements OnInit {

  constructor() {
    super();
  }

  ngOnInit() {
  }

}
