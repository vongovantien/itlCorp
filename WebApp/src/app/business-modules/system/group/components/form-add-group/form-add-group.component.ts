import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, AbstractControl, Validators, FormBuilder } from '@angular/forms';
import { SystemRepo } from 'src/app/shared/repositories';
import { Department } from 'src/app/shared/models/system/department';
import { catchError, finalize, map } from 'rxjs/operators';

@Component({
  selector: 'form-add-group',
  templateUrl: './form-add-group.component.html'
})
export class FormAddGroupComponent extends AppForm implements OnInit {

  formGroup: FormGroup;
  types: CommonInterface.ICommonTitleValue[] = [
    { title: 'Active', value: true },
    { title: 'Inactive', value: false },
  ];
  code: AbstractControl;
  groupNameEN: AbstractControl;
  groupNameVN: AbstractControl;
  groupNameAbbr: AbstractControl;
  department: AbstractControl;
  active: AbstractControl;
  departments: any[] = [];

  constructor(private _fb: FormBuilder,
    private _systemRepo: SystemRepo) {
    super();
  }

  ngOnInit() {
    this.getDepartments();
    this.initForm();
  }
  getDepartments() {
    this._systemRepo.getAllDepartment()
      .pipe(
        catchError(this.catchError),
        finalize(() => { this.isLoading = false; })
      )
      .subscribe(
        (res: any) => {
          this.departments = res;
        },
      );
  }
  initForm() {
    this.formGroup = this._fb.group({
      code: ['', Validators.compose([
        Validators.required,
      ])],
      groupNameEN: ['', Validators.compose([
        Validators.required,
      ])],
      groupNameVN: ['', Validators.compose([
        Validators.required,
      ])],
      groupNameAbbr: ['', Validators.compose([
        Validators.required,
      ])],
      department: [],
      active: [this.types[0]],
    });

    this.code = this.formGroup.controls['code'];
    this.groupNameEN = this.formGroup.controls['groupNameEN'];
    this.groupNameVN = this.formGroup.controls['groupNameVN'];
    this.groupNameAbbr = this.formGroup.controls['groupNameAbbr'];
    this.department = this.formGroup.controls['department'];
    this.active = this.formGroup.controls['active'];
  }
}
