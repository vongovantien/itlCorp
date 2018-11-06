import { Component, OnInit, ViewChild, Input, Output, EventEmitter } from '@angular/core';
import { ModalDirective } from 'ngx-bootstrap';
import { ButtonModalSetting } from '../../models/layout/button-modal-setting.model';
import { ButtonType } from '../../enums/type-button.enum';
import { NgForm } from '@angular/forms';

@Component({
  selector: 'app-modified-modal',
  templateUrl: './modified-modal.component.html',
  styleUrls: ['./modified-modal.component.scss']
})
export class ModifiedModalComponent implements OnInit {
  @Input() model: any = {};
  @Output() save: EventEmitter<any> = new EventEmitter<any>();
  @ViewChild('formAddEdit') form: NgForm;
  @ViewChild("modifiedModal") modifiedModal: ModalDirective;

  saveButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.save
  };
  cancelButtonSetting: ButtonModalSetting = {
    typeButton: ButtonType.cancel
  };
  constructor() { }

  ngOnInit() {
  }
  onSubmit(){
    this.save.emit(this.form);
  }
  onCancel(){
    this.form.onReset();
  }
}
