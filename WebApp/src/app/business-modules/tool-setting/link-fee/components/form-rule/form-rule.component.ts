import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core';
import { PopupBase } from 'src/app/popup.base';
import { SystemRepo, OperationRepo, SettingRepo } from '@repositories';
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

@Component({
    selector: 'form-rule',
    templateUrl: './form-rule.component.html',
})
export class FormRuleComponent extends PopupBase implements OnInit {
    @Output() onUpdate: EventEmitter<boolean> = new EventEmitter<boolean>();
    @Input() rule: RuleLinkFee = new RuleLinkFee();
    formAdd: FormGroup;
    nameRule: AbstractControl;
    userId: AbstractControl;
    serviceBuying: AbstractControl;
    serviceSelling: AbstractControl;
    //chargeBuying: Partial<CommonInterface.IComboGridData> | any = {};
    selectedChargeBuying: Partial<CommonInterface.IComboGridData> | any = {};
    selectedChargeSelling: Partial<CommonInterface.IComboGridData> | any = {};
    selectedPartnerBuying: Partial<CommonInterface.IComboGridData> | any = {};
    selectedPartnerSelling: Partial<CommonInterface.IComboGridData> | any = {};
    title: string = 'Add New Rule Link';

    services: CommonInterface.IComboGirdConfig | any = {};
    configPartner: CommonInterface.IComboGirdConfig | any = {};
    configChargeBuying: CommonInterface.IComboGirdConfig | any = {};
    configChargeSelling: CommonInterface.IComboGirdConfig | any = {};

    expirationDate: AbstractControl;
    effectiveDate: AbstractControl;
    listChargesBuying: Charge[];
    listChargesSelling: Charge[];

    minDateEffective: any = null;
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
        this.formAdd = this._fb.group({
            nameRule: [this.rule.nameRule,Validators.compose([
                Validators.required
            ])],
            serviceBuying: [],
            serviceSelling: [],
            expirationDate: [],
            effectiveDate: [],
        });
        this.serviceBuying = this.formAdd.controls['serviceBuying'];
        this.serviceSelling = this.formAdd.controls['serviceSelling'];
        this.nameRule = this.formAdd.controls['nameRule'];
        this.expirationDate = this.formAdd.controls['expirationDate'];
        this.effectiveDate = this.formAdd.controls['effectiveDate'];
        //this.chargeBuying = this.formAdd.controls['chargeBuying'];\
        //bankCode: [{ value: null, disabled: true }]
        this.formAdd.get("effectiveDate").valueChanges
            .pipe(
                distinctUntilChanged((prev, curr) => prev.endDate === curr.endDate && prev.startDate === curr.startDate),
                map((data: any) => data.startDate)
               
            )
            .subscribe((value: any) => {
                this.minDateExpired = this.createMoment(value); // * Update MinDate -> ExpiredDate.
            });
    }

    initBasicData() {
        this.configPartner = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'PartnerID' },
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
            { displayName: 'Air Export', value: 'AE' },
            { displayName: 'Air Import', value: 'AI' },
            { displayName: 'Sea Consol Export', value: 'SCE' },
            { displayName: 'Sea Consol Import', value: 'SCI' },
            { displayName: 'Sea FCL Export', value: 'SFE' },
            { displayName: 'Sea FCL Import', value: 'SFI' },
            { displayName: 'Sea LCL Export', value: 'SLE' },
            { displayName: 'Sea LCL Import', value: 'SLI' },
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
        const valueForm = this.formAdd.getRawValue();

        const rule: RuleLinkFee = new RuleLinkFee(valueForm);
        if (this.formAdd.invalid) { return; }
        if (!this.isShowUpdate) {
            rule.id = '';
            rule.effectiveDate = formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en'),
                rule.expirationDate = formatDate(this.expirationDate.value.startDate, 'yyyy-MM-dd', 'en'),
                rule.partnerBuying = this.rule.partnerBuying,
                rule.partnerSelling = this.rule.partnerSelling,
                rule.chargeBuying = this.rule.chargeBuying,
                rule.chargeSelling = this.rule.chargeSelling,
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
                            this._toast.error(res.message);
                        });
        } else {
            console.log(this.rule)
            rule.id = this.rule.id,
                rule.effectiveDate = formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en'),
                rule.expirationDate = formatDate(this.expirationDate.value.startDate, 'yyyy-MM-dd', 'en'),
                rule.partnerBuying = this.rule.partnerBuying,
                rule.partnerSelling = this.rule.partnerSelling,
                rule.chargeBuying = this.rule.chargeBuying,
                rule.chargeSelling = this.rule.chargeSelling,
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
        }

    }

}

