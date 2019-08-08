import { Component } from '@angular/core';
import { User, Currency } from 'src/app/shared/models';
import { BaseService } from 'src/app/shared/services';
import { SystemRepo } from 'src/app/shared/repositories';
import { catchError } from 'rxjs/operators';
import { AppForm } from 'src/app/app.form';
import { FormBuilder, FormGroup, AbstractControl } from '@angular/forms';

@Component({
    selector: 'adv-payment-form-create',
    templateUrl: './form-create-advance-payment.component.html'
})

export class AdvancePaymentFormCreateComponent extends AppForm {

    methods: CommonInterface.ICommonTitleValue[] = [];
    selectedMethod: CommonInterface.ICommonTitleValue;

    currencyList: Currency[] = [];
    selectedCurrency: any;

    userLogged: User;

    selectedRequestDate: Date = new Date();
    // ? { Startdate, EndDate }  must be initialized to custom Date.
    selectedDeadLineDate: any = {
        startDate: new Date(new Date().setDate(this.selectedRequestDate.getDate() + 7)), 
        endDate: new Date(new Date().setDate(this.selectedRequestDate.getDate() + 7)), 
    };


    // formCreate: FormGroup;
    // advanceNo: AbstractControl;
    // requester: AbstractControl;
    
    
    constructor(
        private _fb: FormBuilder,
        private _baseService: BaseService,
        private _sysRepo: SystemRepo
    ) {
        super();

    }

    ngOnInit() {
        this.initBasicData();
        this.getUserLogged();
        this.getCurrency();
    }

    initForm() {
        
    }

    initBasicData() {
        this.methods = this.getMethod();
        this.selectedMethod = this.methods[0];
    }

    getMethod(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Cash', value: 'Cash' },
            { title: 'Bank Transer', value: 'Bank' },
        ];
    }

    getUserLogged() {
        this.userLogged = this._baseService.getUserLogin() || 'admin';
    }

    getCurrency() {
        this._sysRepo.getListCurrency()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    this.currencyList = res || [];
                    this.selectedCurrency = this.currencyList.filter((item: Currency) => item.id === 'VND')[0];
                },
                (errors: any) => { },
                () => { }
            );
    }





}
