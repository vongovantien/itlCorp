import { Component, OnInit, Output, EventEmitter } from '@angular/core';

import { Warehouse } from 'src/app/shared/models/catalogue/ware-house.model';
import { PopupBase } from 'src/app/popup.base';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize } from 'rxjs/operators';
import { FormGroup, AbstractControl, FormBuilder, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { NgProgress } from '@ngx-progressbar/core';
import { FormValidators } from 'src/app/shared/validators/form.validator';
import { CommonEnum } from '@enums';
import { AppList } from '@app';

@Component({
    selector: 'user-profile-page',
    templateUrl: './user-profile.component.html',
    styleUrls: ['./user-profile.component.scss']
})
export class UserProfilePageComponent extends AppList {
    //@Output() saveSuccess: EventEmitter<boolean> = new EventEmitter<boolean>();

    fileName: string = null;
    //
    formUser: FormGroup;
    // properties enable update

    employeeNameVn: AbstractControl;
    employeeNameEn: AbstractControl;
    title: AbstractControl;
    email: AbstractControl;
    bankAccountNo: AbstractControl;
    bankName: AbstractControl;
    tel: AbstractControl;
    description: AbstractControl;

    // dump (viewonly) properties

    staffCode: AbstractControl;
    username: AbstractControl;
    workingStatus: AbstractControl;
    creditLimit: AbstractControl;
    creditRate: AbstractControl;

    seedData = {
        employeeNameVn: 'Đoàn Văn Thương',
        employeeNameEn: 'Kenny Thuong',
        title: 'hello',
        email: 'kenny.thuong@gmail.com',
        bankAccountNo: '1288328 93273727 237273',
        bankName: 'Bank',
        tel: '033 5799434',
        description: "I'm a fullstack developer",

        // dump (viewonly) properties

        staffCode: '16110472',
        username: 'kenny.thuong',
        workingStatus: 'Working',
        creditLimit: 16.44,
        creditRate: 24.11
    };

    constructor(
        private _fb: FormBuilder,
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
    }
    initForm() {
        this.formUser = this._fb.group({
            employeeNameVn: [],
            employeeNameEn: [],
            title: [],
            email: [],
            bankAccountNo: [],
            bankName: [],
            tel: [],
            description: [],
            // view only
            staffCode: [],
            username: [],
            workingStatus: [],
            creditLimit: [],
            creditRate: [],
        });
        //
        this.employeeNameVn = this.formUser.controls['employeeNameVn'];
        this.employeeNameEn = this.formUser.controls['employeeNameEn'];
        this.title = this.formUser.controls['title'];
        this.email = this.formUser.controls['email'];
        this.bankAccountNo = this.formUser.controls['bankAccountNo'];
        this.bankName = this.formUser.controls['bankName'];
        this.tel = this.formUser.controls['tel'];
        this.description = this.formUser.controls['description'];

        // view only
        this.staffCode = this.formUser.controls['staffCode'];
        this.username = this.formUser.controls['username'];
        this.workingStatus = this.formUser.controls['workingStatus'];
        this.creditLimit = this.formUser.controls['creditLimit'];
        this.creditRate = this.formUser.controls['creditRate'];

    }

    seed() {

        this.formUser.patchValue({
            employeeNameVn: this.seedData.employeeNameVn,
            employeeNameEn: this.seedData.employeeNameEn,
            title: this.seedData.title,
            email: this.seedData.email,
            bankAccountNo: this.seedData.bankAccountNo,
            bankName: this.seedData.bankName,
            tel: this.seedData.tel,
            description: this.seedData.description,

            // dump (viewonly) properties

            staffCode: this.seedData.staffCode,
            username: this.seedData.username,
            workingStatus: this.seedData.workingStatus,
            creditLimit: this.seedData.creditLimit,
            creditRate: this.seedData.creditRate,
        });

    }

    handleFileInput(event) {
        this.fileName = event.target.value;
        console.log(event.target['files']);
    }

}
