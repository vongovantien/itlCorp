import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { FormGroup, Validators, FormBuilder, AbstractControl } from '@angular/forms';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { Params, ActivatedRoute, Router } from '@angular/router';
import { Group } from 'src/app/shared/models/system/group';
import { NgProgress } from '@ngx-progressbar/core';
import { Department } from 'src/app/shared/models/system/department';

@Component({
  selector: 'app-detail-group',
  templateUrl: './detail-group.component.html',
  styleUrls: ['./detail-group.component.scss']
})
export class GroupDetailComponent extends AppForm implements OnInit {
  formGroup: FormGroup;
  types: CommonInterface.ICommonTitleValue[] = [
    { title: 'Active', value: true },
    { title: 'Inactive', value: false },
  ];
  groupId: number = 0;
  departments: any[] = [];
  code: AbstractControl;
  groupNameEN: AbstractControl;
  groupNameVN: AbstractControl;
  groupNameAbbr: AbstractControl;
  department: AbstractControl;
  officeName: AbstractControl;
  companyName: AbstractControl;
  active: AbstractControl;
  group: Group = null;
  userHeaders: CommonInterface.IHeaderTable[];

  constructor(private _systemRepo: SystemRepo,
    private _router: Router,
    private _progressService: NgProgress,
    private _fb: FormBuilder,
    private _activedRouter: ActivatedRoute) {
    super();
    this._progressRef = this._progressService.ref();
  }

  ngOnInit() {
    this.getDepartments();
    this.initForm();
    this._activedRouter.params.subscribe((param: Params) => {
      if (param.id) {
        this.groupId = Number(param.id);
        this.getGroupDetail(this.groupId);
        this.userHeaders = [
          { title: 'User Name', field: 'userName', sortable: true },
          { title: 'Full Name', field: 'fullName', sortable: true },
          { title: 'Position', field: 'position', sortable: true },
          { title: 'Permission', field: 'permission', sortable: true },
          { title: 'Level Permission', field: 'levelPermission', sortable: true },
          { title: 'Status', field: 'active', sortable: true },
        ];
      }
    });
  }

  getGroupDetail(groupId: number) {
    this._systemRepo.getDetailGroup(groupId)
      .pipe(
        catchError(this.catchError),
        finalize(() => this._progressRef.complete())
      ).subscribe(
        (res: any) => {
          if (res != null) {
            this.group = res;
            this.setValueFormGroup(res);
          } else {
            this.formGroup.reset();
          }
        });
  }
  setValueFormGroup(res: any) {
    this.formGroup.setValue({
      code: res.code,
      groupNameEN: res.nameEn,
      groupNameVN: res.nameVn,
      groupNameAbbr: res.shortName,
      department: this.departments.filter(i => i.id === res.departmentId)[0] || null,
      active: this.types.filter(i => i.value === res.active)[0] || [],
      companyName: res.companyName,
      officeName: res.officeName
    });
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
      officeName: [''],
      companyName: [''],
      active: [this.types[0]],
    });

    this.code = this.formGroup.controls['code'];
    this.groupNameEN = this.formGroup.controls['groupNameEN'];
    this.groupNameVN = this.formGroup.controls['groupNameVN'];
    this.groupNameAbbr = this.formGroup.controls['groupNameAbbr'];
    this.department = this.formGroup.controls['department'];
    this.officeName = this.formGroup.controls['officeName'];
    this.companyName = this.formGroup.controls['companyName'];
    this.active = this.formGroup.controls['active'];
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
  cancel() {
    this._router.navigate(["home/system/group"]);
  }
  update() { }
}
