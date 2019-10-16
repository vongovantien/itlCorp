import { Component, OnInit, ViewChild } from '@angular/core';
import { Location } from '@angular/common';
import { AppForm } from 'src/app/app.form';
import { FormGroup, Validators, FormBuilder, AbstractControl } from '@angular/forms';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { Params, ActivatedRoute, Router } from '@angular/router';
import { Group } from 'src/app/shared/models/system/group';
import { NgProgress } from '@ngx-progressbar/core';
import { Department } from 'src/app/shared/models/system/department';
import { ToastrService } from 'ngx-toastr';
import { FormUserGroupComponent } from '../../components/form-user-group/form-user-group.component';

@Component({
  selector: 'app-detail-group',
  templateUrl: './detail-group.component.html',
  styleUrls: ['./detail-group.component.scss']
})
export class GroupDetailComponent extends AppForm implements OnInit {
  @ViewChild(FormUserGroupComponent, { static: true }) usergroupPopup: FormUserGroupComponent;
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
  users: any[] = null;
  userHeaders: CommonInterface.IHeaderTable[];

  constructor(private _systemRepo: SystemRepo,
    private _router: Router,
    private _progressService: NgProgress,
    private _fb: FormBuilder,
    private _activedRouter: ActivatedRoute,
    private _toastService: ToastrService,
    private _location: Location) {
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
        this.getUsersInGroup(this.groupId);
        this.userHeaders = [
          { title: 'User Name', field: 'userName', sortable: true },
          { title: 'Full Name', field: 'employeeName', sortable: true },
          { title: 'Position', field: 'position', sortable: true },
          { title: 'Permission', field: 'permission', sortable: true },
          { title: 'Level Permission', field: 'levelPermission', sortable: true },
          { title: 'Status', field: 'active', sortable: true }
        ];
      }
    });
  }
  getUsersInGroup(groupId: number) {
    this._systemRepo.getUsersInGroup(groupId)
      .pipe(
        catchError(this.catchError),
        finalize(() => this._progressRef.complete())
      ).subscribe(
        (res: any) => {
          this.users = res;
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
            console.log(this.group);
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
      department: ['', Validators.compose([
        Validators.required
      ])],
      officeName: [{ value: '', disabled: true }, Validators.compose([
        Validators.required
      ])],
      companyName: [{ value: '', disabled: true }, Validators.compose([
        Validators.required
      ])],
      active: [this.types[0], Validators.compose([
        Validators.required
      ])],
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
    this._location.back();
  }
  update() {
    this.isSubmitted = true;
    if (this.formGroup.valid) {
      this._progressRef.start();
      const body: any = {
        id: this.group.id,
        code: this.code.value,
        nameEn: this.groupNameEN.value,
        nameVn: this.groupNameVN.value,
        shortName: this.groupNameAbbr.value,
        departmentId: this.department.value.id,
        active: this.active.value.value
      };
      this._systemRepo.updateGroup(body)
        .pipe(
          catchError(this.catchError),
          finalize(() => this._progressRef.complete())
        )
        .subscribe(
          (res: CommonInterface.IResult) => {
            if (res.status) {
              this._toastService.success(res.message, '');
            } else {
              this._toastService.error(res.message, '');
            }
          }
        );
    }
  }
  addUserToGroup() {
    this.usergroupPopup.show();
  }
  viewUserGroup(item) {
    this.usergroupPopup.title = "Edit/ View User";
    this.usergroupPopup.show();
  }
}
