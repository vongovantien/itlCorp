import { Component, ViewChild, Output, EventEmitter, Input } from '@angular/core';
import { FormGroup, FormBuilder, Validators, AbstractControl } from '@angular/forms';
import { AppForm } from 'src/app/app.form';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { catchError, finalize, shareReplay } from 'rxjs/operators';
import { SalemanPopupComponent } from '../saleman-popup.component';
import { Partner, CountryModel, ProviceModel } from 'src/app/shared/models';
import { FormValidators } from 'src/app/shared/validators/form.validator';
import { PartnerOtherChargePopupComponent } from '../other-charge/partner-other-charge.popup';
import { JobConstants, SystemConstants } from '@constants';
import { forkJoin, Observable } from 'rxjs';
import { THIS_EXPR } from '@angular/compiler/src/output/output_ast';
import { getMenuUserSpecialPermissionState, IAppState } from '@store';
import { Store } from '@ngrx/store';

@Component({
    selector: 'form-add-partner',
    templateUrl: './form-add-partner.component.html'
})

export class FormAddPartnerComponent extends AppForm {

    @ViewChild(SalemanPopupComponent, { static: false }) poupSaleman: SalemanPopupComponent;
    @ViewChild(PartnerOtherChargePopupComponent, { static: false }) otherChargePopup: PartnerOtherChargePopupComponent;

    @Output() requireSaleman = new EventEmitter<boolean>();
    @Input() isUpdate: boolean = false;
    //
    displayFieldCountry: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_COUNTRY;
    displayFieldCity: CommonInterface.IComboGridDisplayField[] = JobConstants.CONFIG.COMBOGRID_CITY_PROVINCE;

    //
    menuSpecialPermission: Observable<any[]>;

    parentCustomers: any[] = [];
    partnerGroups: any[] = [];
    countries: any[];
    shippingProvinces: any[] = [];
    billingProvinces: any[] = [];
    workPlaces: any[] = [];
    isExistedTaxcode: boolean = false;

    nations: Observable<CountryModel[]>;
    billingsProvinces: ProviceModel[] = [];
    shipingsProvinces: ProviceModel[] = [];
    provinces: Observable<ProviceModel[]>;
    //
    countryShippingIdName: string;
    countryIdName: string;
    shippingProvinceName: string;
    billingProvinceName: string;

    partnerForm: FormGroup;
    isSubmitted: boolean = false;
    partnerAccountNo: AbstractControl;
    internalReferenceNo: AbstractControl;
    partnerNameEn: AbstractControl;
    partnerNameVn: AbstractControl;
    shortName: AbstractControl;
    partnerAccountRef: AbstractControl;

    taxCode: AbstractControl;
    partnerGroup: AbstractControl;
    countryShippingId: AbstractControl;
    provinceShippingId: AbstractControl;
    zipCodeShipping: AbstractControl;
    addressShippingEn: AbstractControl;
    addressShippingVn: AbstractControl;
    countryId: AbstractControl;
    provinceId: AbstractControl;
    zipCode: AbstractControl;
    addressEn: AbstractControl;
    addressVn: AbstractControl;
    contactPerson: AbstractControl;
    tel: AbstractControl;
    fax: AbstractControl;
    workPhoneEx: AbstractControl;
    email: AbstractControl;

    bankAccountNo: AbstractControl;
    bankAccountName: AbstractControl;
    bankAccountAddress: AbstractControl;
    swiftCode: AbstractControl;

    active: AbstractControl;
    //
    activePartner: boolean = false;
    //
    note: AbstractControl;
    public: boolean = false;
    coLoaderCode: AbstractControl;
    roundUpMethod: AbstractControl;
    applyDim: AbstractControl;
    //
    billingEmail: AbstractControl;
    billingPhone: AbstractControl;
    //
    groups: string = '';

    roundMethods: CommonInterface.INg2Select[] = [
        { id: 'Standard', text: 'Standard' },
        { id: '0.5', text: 'Round 0.5' },
        { id: '1.0', text: 'Round 1.0' },
    ];

    applyDims: CommonInterface.INg2Select[] = [
        { id: 'Single', text: 'Single Dim' },
        { id: 'Total', text: 'Total Dim' }
    ];


    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<IAppState>
    ) {
        super();
    }
    ngOnInit() {
        //
        //this.activePartner = this.active.value;
        this.menuSpecialPermission = this._store.select(getMenuUserSpecialPermissionState);
        //
        this.initForm();
        this.getCountryProvince();
        console.log("isUpdate: ", this.isUpdate);
        if (this.isUpdate) {
            this.getShippingProvinces();
            this.getBillingProvinces();
        }
        console.log("counttry: ", this.countryShippingId.value);
    }
    getCountryProvince() {
        this.nations = this._catalogueRepo.getCountry();
        this._catalogueRepo.getProvinces()
            .subscribe(
                (provinces: ProviceModel[]) => {
                    this.billingProvinces = this.shippingProvinces = provinces;
                }
            );

    }
    onChange(event: any, type: string) {
        switch (type) {
            case 'countryShippingId':
                this.getShippingProvinces(event.id, !!this.provinceShippingId.value && this.provinceShippingId.value.length > 0 ? this.provinceShippingId.value[0].id : null);


                break;
            case 'countryId':
                this.getBillingProvinces(event.id, !!this.provinceId.value && this.provinceId.value.length > 0 ? this.provinceId.value[0].id : null);
                break;
            case 'category':
                console.log("this.groups before: ", this.partnerGroup.value);
                //if(this.groups === '' ){
                //}
                //this.groups = this.groups + ";" + event.id;
                console.log("event: ", event);

                //let index = [...this.partnerGroup.value].findIndex(e => e.id === 'ALL');

                // có ALL thì về rỗng và set item hiện tại
                if (event.id === 'ALL') {

                    let temp = [{ text: event.text, id: event.id }];
                    this.partnerGroup.setValue([...temp]);
                }
                // không có ALL có 3 TH:
                //TH1: Array không có ALL và Event không = ALL
                //TH2: Array có ALL và Event không = ALL
                //TH3: ... cần thêm length để check
                else {
                    let index = [...this.partnerGroup.value].findIndex(e => e.id === 'ALL');
                    let length = [...this.partnerGroup.value].length;
                    if (index >= 0) {
                        if (length < 2) {
                            console.log(2);
                            let cloneArray = this.partnerGroup.value.filter(e => e.id !== 'ALL');
                            cloneArray.push({ text: event.text, id: event.id });
                            this.partnerGroup.setValue([...cloneArray]);
                        }
                        else {
                            console.log("clicked");
                            let cloneArray = this.partnerGroup.value.filter(e => e.id !== 'ALL');
                            this.partnerGroup.setValue([...cloneArray]);
                        }
                    }
                    else {
                        console.log(3);
                        let newCloneArray = this.partnerGroup.value.map((e) => { return { id: e.id, text: e.text } });
                        this.partnerGroup.setValue(newCloneArray);
                    }
                }


                console.log("this.groups after: ", this.partnerGroup.value);
                const isShowSaleMan = this.checkRequireSaleman();
                this.requireSaleman.emit(isShowSaleMan);
                break;
        }
    }

    removed(event, type: string) {
        switch (type) {
            case 'countryShippingId':
                this.getShippingProvinces();
                this.partnerForm.controls['provinceShippingId'].setValue([]);
                break;
            case 'countryId':
                this.getBillingProvinces();
                this.partnerForm.controls['provinceId'].setValue([]);
                break;
            case 'category':
                this.groups = this.groups.replace(event.id, '');
                const isShowSaleMan = this.checkRequireSaleman();
                this.requireSaleman.emit(isShowSaleMan);
                break;
        }
    }

    checkRequireSaleman(): boolean {

        if (!!this.groups && (this.groups.includes("ALL") || this.groups.includes("CUSTOMER"))) {
            return true;
        } else {
            return false;
        }
    }

    getBillingProvinces(countryId?: number, provinceId: string = null) {
        console.log("co vo", provinceId);
        if (countryId) {
            this._catalogueRepo.getProvincesBycountry(countryId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        //this.billingProvinces = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                        this.billingProvinces = res;
                        if (!!provinceId) {
                            const obj = this.billingProvinces.find(x => x.id === provinceId);
                            console.log(obj)
                            if (obj === undefined) {
                                console.log('clicked')
                                this.provinceId.setValue(null);
                            }
                        }
                    }
                );
        } else {
            this._catalogueRepo.getProvinces()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.billingProvinces = this.utility.prepareNg2SelectData(res || [], 'id', 'name_VN');
                    }
                );
        }
    }

    getShippingProvinces(countryId?: number, provinceId: string = null) {
        console.log("co vo", provinceId);

        if (countryId) {
            this._catalogueRepo.getProvincesBycountry(countryId)
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.shippingProvinces = res;
                        if (!!provinceId) {

                            const obj = this.shippingProvinces.find(x => x.id === provinceId);
                            console.log(obj)
                            if (obj === undefined) {
                                console.log('clicked')
                                this.provinceShippingId.setValue(null);
                            }
                        }
                    }
                );
        } else {
            this._catalogueRepo.getProvinces()
                .pipe(catchError(this.catchError), finalize(() => { }))
                .subscribe(
                    (res) => {
                        this.shippingProvinces = res;
                    }
                );
        }

    }

    initForm() {
        this.partnerForm = this._fb.group({
            partnerAccountNo: [{ value: null, disabled: true }],
            internalReferenceNo: [null, Validators.compose([
                Validators.maxLength(10),
                Validators.minLength(3),
                // Validators.pattern(SystemConstants.CPATTERN.NOT_WHITE_SPACE),
                Validators.pattern(SystemConstants.CPATTERN.TAX_CODE),
            ])],
            partnerNameEn: [null, Validators.compose([

                FormValidators.required,
            ])],
            partnerNameVn: [null, Validators.compose([
                FormValidators.required,
            ])],
            shortName: [null, Validators.compose([
                FormValidators.required
            ])],
            partnerAccountRef: [],
            taxCode: [null, Validators.compose([
                Validators.maxLength(14),
                Validators.minLength(8),
                // Validators.pattern(SystemConstants.CPATTERN.NOT_WHITE_SPACE),
                Validators.pattern(SystemConstants.CPATTERN.TAX_CODE),
                Validators.required
            ])],
            partnerGroup: [null, Validators.compose([
                Validators.required
            ])],
            countryShippingId: [null, Validators.compose([
                Validators.required
            ])],
            provinceShippingId: [null, Validators.compose([
                Validators.required
            ])],
            zipCodeShipping: [null],
            addressShippingEn: [null, Validators.compose([
                FormValidators.required
            ])],
            addressShippingVn: [null, Validators.compose([
                FormValidators.required
            ])],
            countryId: [null, Validators.compose([
                Validators.required
            ])],
            provinceId: [null, Validators.compose([
                Validators.required
            ])],
            zipCode: [null],
            addressEn: [null, Validators.compose([
                FormValidators.required
            ])],
            addressVn: [null, Validators.compose([
                FormValidators.required
            ])],
            //thien
            billingEmail: [null],
            billingPhone: [null],

            //
            contactPerson: [null],
            tel: [null],
            fax: [null],
            workPhoneEx: [null],
            email: [null],

            bankAccountNo: [null],
            bankAccountName: [null],
            bankAccountAddress: [null],
            swiftCode: [null],

            active: [false],
            note: [null],
            coLoaderCode: [null],
            applyDim: [null],
            roundUpMethod: [null]
        });
        //

        //
        this.partnerAccountNo = this.partnerForm.controls['partnerAccountNo'];
        this.internalReferenceNo = this.partnerForm.controls['internalReferenceNo'];
        this.partnerNameEn = this.partnerForm.controls['partnerNameEn'];
        this.partnerNameVn = this.partnerForm.controls['partnerNameVn'];
        this.shortName = this.partnerForm.controls['shortName'];
        this.partnerAccountRef = this.partnerForm.controls['partnerAccountRef'];
        this.taxCode = this.partnerForm.controls['taxCode'];
        this.partnerGroup = this.partnerForm.controls['partnerGroup'];
        this.countryShippingId = this.partnerForm.controls['countryShippingId'];
        this.provinceShippingId = this.partnerForm.controls['provinceShippingId'];
        this.zipCodeShipping = this.partnerForm.controls['zipCodeShipping'];
        this.addressShippingEn = this.partnerForm.controls['addressShippingEn'];
        this.addressShippingVn = this.partnerForm.controls['addressShippingVn'];
        this.countryId = this.partnerForm.controls['countryId'];
        this.provinceId = this.partnerForm.controls['provinceId'];
        this.zipCode = this.partnerForm.controls['zipCode'];
        this.addressEn = this.partnerForm.controls['addressEn'];
        this.addressVn = this.partnerForm.controls['addressVn'];
        this.contactPerson = this.partnerForm.controls['contactPerson'];
        this.tel = this.partnerForm.controls['tel'];
        this.fax = this.partnerForm.controls['fax'];
        this.workPhoneEx = this.partnerForm.controls['workPhoneEx'];
        this.email = this.partnerForm.controls['email'];

        this.bankAccountNo = this.partnerForm.controls['bankAccountNo'];
        this.bankAccountName = this.partnerForm.controls['bankAccountName'];
        this.bankAccountAddress = this.partnerForm.controls['bankAccountAddress'];
        this.swiftCode = this.partnerForm.controls['swiftCode'];

        this.active = this.partnerForm.controls['active'];
        this.coLoaderCode = this.partnerForm.controls['coLoaderCode'];
        this.note = this.partnerForm.controls['note'];
        this.applyDim = this.partnerForm.controls['applyDim'];
        this.roundUpMethod = this.partnerForm.controls['roundUpMethod'];
        //
        this.billingEmail = this.partnerForm.controls['billingEmail'];
        this.billingPhone = this.partnerForm.controls['billingPhone'];
        //
        this.activePartner = this.active.value;
    }

    /*onSelectDataFormInfo(data: any) {
        this.partnerAccountRef.setValue(data.id);
    }*/
    onSelectDataFormInfo(data: any, type: string) {

        switch (type) {
            case 'acRef':
                this.partnerAccountRef.setValue(data.id);
                break;
            case 'shippping-country':
                //console.log(data.nameVn, type);
                this.countryShippingId.setValue(data.id);
                this.shippingProvinceName = null;
                //set value bat ky khac null.
                this.provinceShippingId.setValue(data.id);
                this.getShippingProvinces(data.id, !!this.provinceShippingId.value ? this.provinceShippingId.value : null);
                break;
            case 'shippping-city':
                this.provinceShippingId.setValue(data.id);
                break;
            case 'billing-country':
                //console.log(data.nameVn, type);
                this.countryId.setValue(data.id);
                //
                this.billingProvinceName = null;
                //set value bat ky khac null.
                this.provinceId.setValue(data.id);
                // this.getBillingProvince(data.id);
                this.getBillingProvinces(data.id, !!this.provinceId.value ? this.provinceId.value : null);
                break;
            case 'billing-city':
                this.provinceId.setValue(data.id);

                break;
            default:
                break;
        }
    }

    setFormData(partner: Partner) {
        //set Name Country
        this.countryShippingIdName = partner.countryShippingName;
        this.countryIdName = partner.countryName;
        //set Name Province
        this.shippingProvinceName = partner.provinceShippingName;
        this.billingProvinceName = partner.provinceName;

        if (partner.countryId && partner.countryShippingId) {
            this.getShippingProvinces(partner.countryShippingId);
            this.getBillingProvinces(partner.countryId);
        }
        //console.log('shipping province: ', this.shippingProvince.value);
        //console.log('billing province: ', this.billingProvince.value);
        //

        // this.countryShippingIdId = partner.countryShippingId;
        // this.countryIdId = partner.countryId;
        // console.log("shipping Id: ", this.shippingProvinceName);
        // console.log("billing Id: ", this.billingProvinceName);

        console.log("partner: ", partner);
        const isShowSaleMan = this.checkRequireSaleman();
        this.requireSaleman.emit(isShowSaleMan);
        const partnerGroupActives = !!partner.partnerGroup ? this.getPartnerGroupActives(partner.partnerGroup.split(';')) : null;
        let index = -1;
        let parentCustomerActive = [];
        let workPlaceActive = [];
        let countryIdActive = [];
        let countryShippingIdActive = [];
        let billingProvinceActive = [];
        let shippingProvinceActive = [];
        index = this.parentCustomers.findIndex(x => x.id === partner.parentId);
        if (index > -1) { parentCustomerActive = [this.parentCustomers[index]]; }

        index = this.countries.findIndex(x => x.id === partner.countryId);
        if (index > - 1) {
            countryIdActive = [this.countries[index]];
        }
        index = this.countries.findIndex(x => x.id === partner.countryShippingId);
        if (index > -1) {
            countryShippingIdActive = [this.countries[index]];
        }
        index = this.billingProvinces.findIndex(x => x.id === partner.provinceId);
        if (index > -1) {
            billingProvinceActive = [this.billingProvinces[index]];
        }
        index = this.shippingProvinces.findIndex(x => x.id === partner.provinceShippingId);
        if (index > -1) {
            shippingProvinceActive = [this.shippingProvinces[index]];
        }
        this.public = partner.public;

        this.partnerForm.setValue({
            partnerAccountNo: partner.accountNo,
            internalReferenceNo: partner.internalReferenceNo,
            partnerNameEn: partner.partnerNameEn,
            partnerNameVn: partner.partnerNameVn,
            shortName: partner.shortName,
            partnerAccountRef: partner.parentId,
            taxCode: partner.taxCode,
            partnerGroup: partnerGroupActives,
            //countryShippingId: countryShippingIdActive,
            countryShippingId: partner.countryShippingId,
            //shippingProvince: shippingProvinceActive,
            provinceShippingId: partner.provinceShippingId,


            zipCodeShipping: partner.zipCodeShipping,
            addressShippingEn: partner.addressShippingEn,
            addressShippingVn: partner.addressShippingVn,
            // countryId: countryIdActive,
            countryId: partner.countryId,
            //billingProvince: billingProvinceActive,
            provinceId: partner.provinceId,

            zipCode: partner.zipCode,
            addressEn: partner.addressEn,
            addressVn: partner.addressVn,
            //
            billingEmail: partner.billingEmail,
            billingPhone: partner.billingPhone,
            //
            contactPerson: partner.contactPerson,
            tel: partner.tel,
            fax: partner.fax,
            workPhoneEx: partner.workPhoneEx,
            email: partner.email,

            bankAccountNo: partner.bankAccountNo,
            bankAccountName: partner.bankAccountName,
            bankAccountAddress: partner.bankAccountAddress,
            swiftCode: partner.swiftCode,

            active: partner.active === null ? false : partner.active,
            note: partner.note,
            coLoaderCode: partner.coLoaderCode,
            roundUpMethod: [<CommonInterface.INg2Select>{ id: partner.roundUpMethod, text: partner.roundUpMethod }],
            applyDim: [<CommonInterface.INg2Select>{ id: partner.applyDim, text: partner.applyDim }]
        });

        //console.log(this.partnerForm.value);
    }

    getPartnerGroupActives(arg0: string[]): any {
        const partnerGroupActives = [];
        if (arg0.length > 0) {
            for (let i = 0; i < arg0.length; i++) {
                const group = this.partnerGroups.find(x => x.id === arg0[i]);
                if (group) {
                    partnerGroupActives.push(group);
                }
            }
        }
        return partnerGroupActives;
    }

    showPopupSaleman() {
        this.poupSaleman.isSave = false;
        this.poupSaleman.isDetail = false;
        this.poupSaleman.show();
    }

    showOtherCharge() {
        this.otherChargePopup.show();
    }

    copyShippingAddress() {
        this.countryId.setValue(this.countryShippingId.value);
        this.provinceId.setValue(this.provinceShippingId.value);
        this.getBillingProvinces(!!this.countryShippingId.value && this.countryShippingId.value.length > 0 ? this.countryShippingId.value[0].id : null, !!this.provinceId.value && this.provinceShippingId.value.length > 0 ? this.provinceId.value[0].id : null);
        this.zipCode.setValue(this.zipCodeShipping.value);
        this.addressEn.setValue(this.addressShippingEn.value);
        this.addressVn.setValue(this.addressShippingVn.value);
        //

    }
}