import { Component } from '@angular/core';
import { AppPage } from 'src/app/app.base';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { ToastrService } from 'ngx-toastr';
import { Router } from '@angular/router';
import { NgProgress } from '@ngx-progressbar/core';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';
import { Department } from 'src/app/shared/models/system/department';

@Component({
    selector: 'app-department-new',
    templateUrl: './add-department.component.html'
})

export class DepartmentAddNewComponent extends AppPage {
    formAdd: FormGroup;
    departmentCode: AbstractControl;
    nameEn: AbstractControl;
    nameLocal: AbstractControl;
    nameAbbr: AbstractControl;
    office: AbstractControl;
    status: AbstractControl;
    officeList: any[] = [];

    isValidForm: boolean = false;
    isSubmited: boolean = false;

    constructor(
        private _systemRepo: SystemRepo,
        private _toastService: ToastrService,
        private _router: Router,
        private _progressService: NgProgress,
        private _fb: FormBuilder,
    ) {
        super();
        this._progressRef = this._progressService.ref();
    }

    ngOnInit() {
        this.initDataInform();
        this.initForm();
    }

    initForm() {
        this.formAdd = this._fb.group({
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
            status: []
        });

        this.departmentCode = this.formAdd.controls['departmentCode'];
        this.nameEn = this.formAdd.controls['nameEn'];
        this.nameLocal = this.formAdd.controls['nameLocal'];
        this.nameAbbr = this.formAdd.controls['nameAbbr'];
        this.office = this.formAdd.controls['office'];
        this.status = this.formAdd.controls['status'];

        this.status.setValue(true);
    }

    initDataInform() {
        this.getOffices();
    }

    saveDepartment() {
        this.isSubmited = true;
        if (this.formAdd.valid) {
            const dept: Department = {
                id: 0,
                code: this.departmentCode.value,
                deptName: this.nameLocal.value,
                deptNameEn: this.nameEn.value,
                deptNameAbbr: this.nameAbbr.value,
                branchId: this.office.value[0].id,
                officeName: this.office.value[0].text,
                companyName: '',
                managerId: '',
                userCreated: '',
                datetimeCreated: '',
                userModified: '',
                datetimeModified: '',
                active: this.status.value,
                inactiveOn: '',
                companyId: null
            };
            this._progressRef.start();
            //Add new Department
            this._systemRepo.addNewDepartment(dept)
                .pipe(catchError(this.catchError), finalize(() => this._progressRef.complete()))
                .subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toastService.success(res.message);
                            this._router.navigate([`home/system/department/${res.data.id}`]);
                        } else {
                            this._toastService.error(res.message);
                        }
                    }
                );
        }
    }

    getOffices() {
        this._systemRepo.getAllOffice()
            .pipe(
                catchError(this.catchError),
                finalize(() => this._progressRef.complete())
            )
            .subscribe(
                (data: any) => {
                    this.officeList = data.map((item: any) => ({ "id": item.id, "text": item.branchNameEn }));
                    console.log(this.officeList)
                },
            );
    }

    back() {
        this._router.navigate(['home/system/department']);
    }

}


