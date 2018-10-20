import { Component, OnInit, Input } from '@angular/core';
import { ButtonModalSetting, ButtonAttributeSetting } from '../../models/layout/button-modal-setting.model';
import { ButtonType } from '../../enums/type-button.enum';
import { SelectModule } from 'ng2-select';
import { AddDefaultButton, EditDefaultButton, DeleteDefaultButton, ImportDefaultButton, ExportDefaultButton, SaveDefaultButton, CancelDefaultButton, ResetDefaultButton } from '../../enums/default-button-enum';

@Component({
  selector: 'app-addmodal-button',
  templateUrl: './addmodal-button.component.html',
  styleUrls: ['./addmodal-button.component.scss']
})
export class AddmodalButtonComponent implements OnInit {
  @Input() buttonSetting: ButtonModalSetting;
  @Input() dataTarget: string;
  isAdd: boolean = false;
  isEdit: boolean = false;
  isDelete: boolean = false;
  isImport: boolean = false;
  isExport: boolean= false;
  isSave: boolean = false;
  isCancel: boolean = false;
  buttonAttribute: ButtonAttributeSetting;

  constructor() { }

  ngOnInit() {
    this.buttonAttribute = this.buttonSetting.buttonAttribute;
    this.setMode(this.buttonSetting.typeButton);
  }

  setMode(typeButton: ButtonType): any {
    if(typeButton == ButtonType.add){
      this.isAdd = true;
      this.setSyleButton(AddDefaultButton);
    }
    if(typeButton == ButtonType.edit){
      this.isEdit = true;
      this.setSyleButton(EditDefaultButton);
    }
    if(typeButton == ButtonType.delete){
      this.isDelete = true;
      this.setSyleButton(DeleteDefaultButton);
    }
    if(typeButton == ButtonType.import){
      this.isImport = true;
      this.setSyleButton(ImportDefaultButton);
    }
    if(typeButton == ButtonType.export){
      this.isExport = true;
      this.setSyleButton(ExportDefaultButton);
    }
    if(typeButton == ButtonType.save){
      this.isSave = true;
      this.setSyleButton(SaveDefaultButton);
    }
    if(typeButton == ButtonType.cancel){
      this.isCancel = true;
      this.setSyleButton(CancelDefaultButton);
    }
    if(typeButton == ButtonType.reset){
      this.isCancel = true;
      this.setSyleButton(ResetDefaultButton);
    }
  }
  setSyleButton(DefaultButton: ButtonAttributeSetting): any {
    if(this.buttonAttribute == null){
      this.buttonAttribute = DefaultButton;
    }
  }
}
