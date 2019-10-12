import { Component, ViewChild } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { SystemRepo } from 'src/app/shared/repositories';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { catchError, finalize, tap, switchMap } from 'rxjs/operators';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { Department } from 'src/app/shared/models/system/department';
import { Office } from 'src/app/shared/models/system/office';
import { Group } from 'src/app/shared/models/system/group';
import { SortService } from 'src/app/shared/services';
import { AppList } from 'src/app/app.list';

@Component({
    selector: 'app-department-detail',
    templateUrl: './detail-department.component.html'
})

export class DepartmentDetailComponent extends AppList {
    //@ViewChild(SettlementFormCreateComponent, { static: false }) formCreateSurcharge: SettlementFormCreateComponent;
    formDetail: FormGroup;
    departmentCode: AbstractControl;
    nameEn: AbstractControl;
    nameLocal: AbstractControl;
    nameAbbr: AbstractControl;
    office: AbstractControl;
    company: AbstractControl;
    status: AbstractControl;
    statusList: CommonInterface.ICommonTitleValue[] = [];
    officeList: Office[];

    isValidForm: boolean = false;
    isSubmited: boolean = false;

    departmentId: number = 0;
    department: Department;

    grpHeaders: CommonInterface.IHeaderTable[];
    userHeaders: CommonInterface.IHeaderTable[];

    groups: Group[] = [];

    constructor(
        private _activedRouter: ActivatedRoute,
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _fb: FormBuilder,
        private _progressService: NgProgress,
        private _sortService: SortService,
    ) {
        super();
        this._progressRef = this._progressService.ref();
        this.requestSort = this.sortGroup;
    }

    ngOnInit() {
        this._activedRouter.params.subscribe((param: Params) => {
            if (param.id) {
                this.departmentId = param.id;
                this.getStatus();
                this.initForm();
                this.getDetail();
                this.getGroupsByDeptId(this.departmentId);

                this.grpHeaders = [
                    { title: 'Group Code', field: 'code', sortable: true },
                    { title: 'Name EN', field: 'nameEn', sortable: true },
                    { title: 'Name Local', field: 'nameVn', sortable: true },
                    { title: 'Name Abbr', field: 'shortName', sortable: true },
                    { title: 'Status', field: 'active', sortable: true },
                ];
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

    initForm() {
        this.formDetail = this._fb.group({
            departmentCode: ['',
                Validators.compose([
                    Validators.required,
                    Validators.maxLength(50)
                ])
            ],
            nameEn: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            nameLocal: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            nameAbbr: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            office: ['',
                Validators.compose([
                    Validators.required
                ])
            ],
            company: [],
            status: [this.statusList[0]]
        });

        this.departmentCode = this.formDetail.controls['departmentCode'];
        this.nameEn = this.formDetail.controls['nameEn'];
        this.nameLocal = this.formDetail.controls['nameLocal'];
        this.nameAbbr = this.formDetail.controls['nameAbbr'];
        this.office = this.formDetail.controls['office'];
        this.company = this.formDetail.controls['company'];
        this.status = this.formDetail.controls['status'];
    }

    updateDepartment() {
        this.isSubmited = true;
        if (this.formDetail.valid) {
            const dept: Department = {
                id: this.departmentId,
                code: this.departmentCode.value,
                deptName: this.nameLocal.value,
                deptNameEn: this.nameEn.value,
                deptNameAbbr: this.nameAbbr.value,
                branchId: this.office.value.id,
                officeName: this.office.value.branchNameEn,
                companyName: '',
                managerId: '',
                userCreated: '',
                datetimeCreated: '',
                userModified: '',
                datetimeModified: '',
                active: this.status.value.value,
                inactiveOn: ''
            };
            //console.log(dept);
            //console.log(this.formDetail.value)
            this._progressRef.start();
            //Update Info Department
            this._systemRepo.updateDepartment(dept)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this.getDetail();
                        } else {
                            this._toastService.error(res.message);
                        }

                    }
                );
        }
    }

    back() {
        this._router.navigate(['home/system/department']);
    }

    getStatus() {
        this.statusList = [
            { title: 'Active', value: true },
            { title: 'Inactive', value: false }
        ];
    }

    getDetail() {
        this._progressRef.start();
        this._systemRepo.getAllOffice()
            .pipe(
                catchError(this.catchError),
                tap(data => {
                    this.officeList = data.map((item: any) => ({ id: item.id, code: item.code, branchname_En: item.branchNameEn }));
                    console.log(this.officeList)
                }),
                switchMap(() => this._systemRepo.getDetailDepartment(this.departmentId).pipe(
                    catchError(this.catchError),
                    finalize(() => {
                        this._progressRef.complete();
                    })
                ))
            )
            .subscribe(
                (res: any) => {
                    //console.log(res)
                    if (res.id !== 0) {
                        this.department = new Department(res);
                        console.log(this.department);
                        this.formDetail.setValue({
                            departmentCode: res.code,
                            nameEn: res.deptNameEn,
                            nameLocal: res.deptName,
                            nameAbbr: res.deptNameAbbr,
                            office: this.officeList.filter(i => i.id === res.branchId)[0] || [],
                            company: res.companyName,
                            status: this.statusList.filter(i => i.value === res.active)[0] || [],
                        });
                    } else {
                        //Reset 
                        this.formDetail.reset();
                        this._toastService.error("Not found data");
                    }
                },
            );
    }

    getGroupsByDeptId(id: number) {
        this._systemRepo.getGroupsByDeptId(id)
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (data: any) => {
                    console.log(data);
                    this.groups = data;
                    console.log(this.groups)
                },
            );
    }

    sortGroup(sort: string): void {
        this.groups = this._sortService.sort(this.groups, sort, this.order);
    }

    gotoDetailGroup(id: number){
        console.log(`Go to Group detail of ${id}`)
    }
}


