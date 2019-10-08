import { Component, OnInit } from '@angular/core';
import { AppList } from 'src/app/app.list';
import { ButtonModalSetting } from '../../../shared/models/layout/button-modal-setting.model';
import { ButtonType } from '../../../shared/enums/type-button.enum';
import { SystemRepo } from 'src/app/shared/repositories';

@Component({
  selector: 'app-office',
  templateUrl: './office.component.html'
})
export class OfficeComponent extends AppList implements OnInit {
  importButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.import
  };
  exportButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.export
  };
  saveButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.save
  };

  cancelButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.cancel
  };
  addButtonSetting: ButtonModalSetting = {
    dataTarget: "add-ware-house-modal",
    typeButton: ButtonType.add
  };
  constructor(
    private _systemRepo: SystemRepo

  ) {
    super();
  }

  ngOnInit() {

  }

}
