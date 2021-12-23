import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SettingRepo } from '@repositories';
import { FormBuilder, FormGroup, AbstractControl, Validators } from '@angular/forms';
import { ToastrService } from 'ngx-toastr';
import { Charge, Partner, Tariff, User } from '@models';
import { RuleLinkFee } from 'src/app/shared/models/tool-setting/rule-link-fee';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, distinctUntilChanged, map } from 'rxjs/operators';
import { DataService } from '@services';
import { SystemConstants } from 'src/constants/system.const';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { formatDate } from '@angular/common';
import { ChargeConstants } from '@constants';

@Component({
    selector: 'form-rule',
    templateUrl: './form-rule.component.html',
})
export class FormRuleComponent extends PopupBase implements OnInit {
    @Output() onUpdate: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Input() rule: RuleLinkFee = new RuleLinkFee();
    formGroup: FormGroup;
    ruleName: AbstractControl;
    serviceBuying: AbstractControl;
    serviceSelling: AbstractControl;
    status: AbstractControl;

    selectedChargeBuying: Partial<CommonInterface.IComboGridData> | any = {};
    selectedChargeSelling: Partial<CommonInterface.IComboGridData> | any = {};
    selectedPartnerBuying: Partial<CommonInterface.IComboGridData> | any = {};
    selectedPartnerSelling: Partial<CommonInterface.IComboGridData> | any = {};
    title: string = 'Add New Rule Link';

    services: CommonInterface.IComboGirdConfig | any = {};
    configPartner: CommonInterface.IComboGirdConfig | any = {};
    configChargeBuying: CommonInterface.IComboGirdConfig | any = {};
    configChargeSelling: CommonInterface.IComboGirdConfig | any = {};

    expiredDate: AbstractControl;
    effectiveDate: AbstractControl;

    minDateEffective: any = null;
    maxDateEffective: any = null;
    minDateExpired: any = null;

    datetimeCreated: string;
    userNameCreated: string;
    userNameModified: string;
    datetimeModified: string;

    isBuying: boolean = true;
    isSelling: boolean = true;

    constructor(
        private _fb: FormBuilder,
        private _toast: ToastrService,
        protected _catalogueRepo: CatalogueRepo,
        private _dataService: DataService,
        private _settingRepo: SettingRepo,
    ) {
        super();
    }

    ngOnInit(): void {
        this.initForm();
        this.initBasicData();
        this.getService();
        this.getPartner();
        this.getChargeBuying();
        this.getChargeSelling();
    }

    initForm() {
        this.formGroup = this._fb.group({
            ruleName: [null, Validators.compose([
                Validators.required
            ])],
            serviceBuying: [null, Validators.compose([
                Validators.required
            ])],
            serviceSelling: [null, Validators.compose([
                Validators.required
            ])],
            effectiveDate: [null, Validators.compose([
                Validators.required
            ])],
            expiredDate: [null],
            status: [true]
        });
        this.serviceBuying = this.formGroup.controls['serviceBuying'];
        this.serviceSelling = this.formGroup.controls['serviceSelling'];
        this.ruleName = this.formGroup.controls['ruleName'];
        this.expiredDate = this.formGroup.controls['expiredDate'];
        this.effectiveDate = this.formGroup.controls['effectiveDate'];
        this.status = this.formGroup.controls['status'];
        this.formGroup.get("effectiveDate").valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                map((data: any) => data.startDate)

            )
            .subscribe((value: any) => {
                this.minDateExpired = this.createMoment(value); // * Update MinDate -> ExpiredDate.
            });
        this.formGroup.get("expiredDate").valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                map((data: any) => data.startDate)

            )
            .subscribe((value: any) => {
                this.maxDateEffective = this.createMoment(value); // * Update MinDate -> ExpiredDate.
            });

    }

    initBasicData() {
        this.configPartner = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'taxCode', label: 'Partner Code' },
                { field: 'shortName', label: 'Abbr Name' },
                { field: 'partnerNameEn', label: 'Name EN' },

            ]
        }, { selectedDisplayFields: ['shortName'], });

        this.configChargeBuying = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'chargeNameEn', label: 'Name' },
                { field: 'unitPrice', label: 'Unit Price' },
                { field: 'unit', label: 'Unit' },
                { field: 'code', label: 'Code' },
            ]
        }, { selectedDisplayFields: ['chargeNameEn'], });

        this.configChargeSelling = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'chargeNameEn', label: 'Name' },
                { field: 'unitPrice', label: 'Unit Price' },
                { field: 'unit', label: 'Unit' },
                { field: 'code', label: 'Code' },
            ]
        }, { selectedDisplayFields: ['chargeNameEn'], });
    }

    getService() {
        this.services = [
            { text: 'Custom Logistic', id: 'CL' },
            { text: 'Air Export', id: 'AE' },
            { text: 'Air Import', id: 'AI' },
            { text: 'Sea Consol Export', id: 'SCE' },
            { text: 'Sea Consol Import', id: 'SCI' },
            { text: 'Sea FCL Export', id: 'SFE' },
            { text: 'Sea FCL Import', id: 'SFI' },
            { text: 'Sea LCL Export', id: 'SLE' },
            { text: 'Sea LCL Import', id: 'SLI' },
            { text: 'Inland Trucking', id: 'IT' },
        ];
    }


    getPartner() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.PARTNER)) {
            this.configPartner.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.PARTNER) || [];
        } else {
            this._catalogueRepo.getListPartner(null, null, { active: true })
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataPartner: any = []) => {
                        this.configPartner.dataSource = dataPartner || [];
                        this._dataService.setDataService(SystemConstants.CSTORAGE.PARTNER, dataPartner || []);
                    },
                );
        }
    }
    getChargeBuying() {
        console.log(this.serviceBuying.value);
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CHARGE)) {
            this.configChargeBuying.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CHARGE) || [];
        } else {
            this._catalogueRepo.getCharges({ active: true, serviceTypeId: this.serviceBuying.value, type: CommonEnum.CHARGE_TYPE.CREDIT })
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataCharge: any = []) => {
                        this.configChargeBuying.dataSource = dataCharge;
                        this._dataService.setDataService(SystemConstants.CSTORAGE.CHARGE, dataCharge || []);
                    },
                );
        }
    }

    getChargeSelling() {
        if (!!this._dataService.getDataByKey(SystemConstants.CSTORAGE.CHARGE)) {
            this.configChargeSelling.dataSource = this._dataService.getDataByKey(SystemConstants.CSTORAGE.CHARGE);
        } else {
            this._catalogueRepo.getCharges({ active: true, serviceTypeId: this.serviceSelling.value, type: CommonEnum.CHARGE_TYPE.DEBIT })
                .pipe(catchError(this.catchError))
                .subscribe(
                    (dataCharge: any = []) => {
                        this.configChargeSelling.dataSource = dataCharge;
                        this._dataService.setDataService(SystemConstants.CSTORAGE.CHARGE, dataCharge);
                    },
                );
        }
    }
    enableSelling() {
        this.isSelling = false;
    }
    enableBuying() {
        this.isBuying = false;
    }
    onSelectDataFormInfo(data: Charge | Partner | any, key: string | any) {
        switch (key) {
            case 'chargeBuying':
                this.selectedChargeBuying = { field: 'shortName', value: data.chargeNameEn, data: data };
                this.rule.chargeBuying = data.id;
                break;
            case 'chargeSelling':
                this.selectedChargeSelling = { field: 'shortName', value: data.chargeNameEn, data: data };
                this.rule.chargeSelling = data.id;
                break;
            case 'partnerBuying':
                this.selectedPartnerBuying = { field: 'shortName', value: data.shortName, data: data };
                this.rule.partnerBuying = data.id;
                break;
            case 'partnerSelling':
                this.selectedPartnerSelling = { field: 'shortName', value: data.shortName, data: data };
                this.rule.partnerSelling = data.id;
                break;
            default:
                break;
        }
    }

    onSaveRule() {
        this.isSubmitted = true;
        const valueForm = this.formGroup.getRawValue();
        const rule: RuleLinkFee = new RuleLinkFee(valueForm);
        if(!this.selectedChargeBuying.value||!this.selectedChargeSelling.value||!this.selectedPartnerBuying.value||!this.selectedPartnerSelling.value||!this.effectiveDate.value.startDate){
            return;
        }
        if(!this.expiredDate.value.startDate){
            rule.expiredDate=null
        }else{
            rule.expiredDate = formatDate(this.expiredDate.value.startDate, 'yyyy-MM-dd', 'en')
        }
        if (this.formGroup.invalid) { return; }
        if (!this.isShowUpdate) {
            rule.id = '';
            rule.effectiveDate = formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en')
                rule.partnerBuying = this.rule.partnerBuying,
                rule.partnerSelling = this.rule.partnerSelling,
                rule.chargeBuying = this.rule.chargeBuying,
                rule.chargeSelling = this.rule.chargeSelling,
                rule.serviceBuying=rule.serviceBuying.id,
                rule.serviceSelling=rule.serviceSelling.id,
                this._settingRepo.addRule(rule)
                    .subscribe(
                        (res: CommonInterface.IResult) => {
                            if (res.status) {

                                this._toast.success(res.message);
                                this.isSubmitted = false;
                                this.onUpdate.emit(true);
                                this.hide();
                                return;
                            }
                            console.log('running');
                            this._toast.error(res.message);
                        });
                        console.log('submit '+ rule);
                        
        } else {
            rule.id = this.rule.id,
                rule.effectiveDate = this.effectiveDate.value ? (this.effectiveDate.value.startDate !== null ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null) : null,
                rule.partnerBuying = this.rule.partnerBuying,
                rule.partnerSelling = this.rule.partnerSelling,
                rule.chargeBuying = this.rule.chargeBuying,
                rule.chargeSelling = this.rule.chargeSelling,
                rule.serviceBuying=rule.serviceBuying.id,
                rule.serviceSelling=rule.serviceSelling.id,
                this._settingRepo.updateRule(rule).subscribe(
                    (res: CommonInterface.IResult) => {
                        if (res.status) {
                            this._toast.success(res.message);
                            this.isSubmitted = false;
                            this.onUpdate.emit(true);
                            this.hide();
                            return;
                        }

                        this._toast.error(res.message);
                    });
                    console.log('update '+ rule);
        }

    }

    resetExpiredDate(){
        this.expiredDate.setValue(null);
    }
}

