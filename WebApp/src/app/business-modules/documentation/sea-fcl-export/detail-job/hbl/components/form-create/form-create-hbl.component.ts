import { Component, OnInit } from '@angular/core';
import { AppForm } from 'src/app/app.form';
import { Observable } from 'rxjs';
import { Customer } from 'src/app/shared/models/catalogue/customer.model';
import { CatalogueRepo, SystemRepo, DocumentationRepo } from 'src/app/shared/repositories';
import { CommonEnum } from 'src/app/shared/enums/common.enum';
import { User } from 'src/app/shared/models';
import { FormGroup, AbstractControl, FormBuilder } from '@angular/forms';
import { catchError } from 'rxjs/operators';
import { DataService } from 'src/app/shared/services';

@Component({
    selector: 'form-create-hbl-fcl-export',
    templateUrl: './form-create-hbl.component.html'
})

export class SeaFCLExportFormCreateHBLComponent extends AppForm implements OnInit {

    formCreate: FormGroup;
    customer: AbstractControl;
    saleMan: AbstractControl;
    shipper: AbstractControl;
    shipperDescription: AbstractControl;
    consignee: AbstractControl;
    notifyParty: AbstractControl;
    mawb: AbstractControl;
    consigneeDescription: AbstractControl;
    notifyPartyDescription: AbstractControl;
    hblNo: AbstractControl;
    mbltype: AbstractControl;
    bookingNo: AbstractControl;
    localVessel: AbstractControl;
    oceanVessel: AbstractControl;
    country: AbstractControl;
    placeReceipt: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;
    placeDelivery: AbstractControl;
    finalDestination: AbstractControl;
    freightCharge: AbstractControl;



    customers: Observable<Customer[]>;
    saleMans: User[];
    shipppers: Observable<Customer[]>;
    consignees: Observable<Customer[]>;


    serviceTypes: CommonInterface.INg2Select[];
    ladingTypes: CommonInterface.INg2Select[];
    termTypes: CommonInterface.INg2Select[];

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'partnerNameEn', label: 'Name ABBR' },
        { field: 'partnerNameVn', label: 'Name EN' },
        { field: 'taxCode', label: 'Tax Code' },
    ];

    constructor(
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder,
        private _dataService: DataService,
        private _documentRepo: DocumentationRepo
    ) {
        super();
    }

    ngOnInit() {
        this.initForm();
        this.getCustomer();
        this.getSaleMans();
        this.getShippers();
        this.getConsignees();
        this.getCommonData();
    }

    initForm() {
        this.formCreate = this._fb.group({
            // * Combogrid
            customer: [],
            saleMan: [],
            shipper: [],
            consignee: [],
            notifyParty: [],
            // * Select
            mbltype: [],

            shipperDescription: [],
            consigneeDescription: [],
            notifyPartyDescription: [],
            bookingNo: []

        });

        this.customer = this.formCreate.controls["customer"];
        this.saleMan = this.formCreate.controls["saleMan"];
        this.shipper = this.formCreate.controls["shipper"];
        this.shipperDescription = this.formCreate.controls["shipperDescription"];
        this.consignee = this.formCreate.controls["consignee"];
        this.notifyParty = this.formCreate.controls["notifyParty"];
        this.consigneeDescription = this.formCreate.controls["consigneeDescription"];
        this.notifyPartyDescription = this.formCreate.controls["notifyPartyDescription"];
        this.mbltype = this.formCreate.controls["mbltype"];
        this.bookingNo = this.formCreate.controls["bookingNo"];
    }

    getCustomer() {
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER);
    }

    getSaleMans() {
        this._systemRepo.getListSystemUser()
            .pipe(catchError(this.catchError))
            .subscribe((res: any) => {
                this.saleMans = res || [];
            });
    }

    getShippers() {
        this.shipppers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.SHIPPER);
    }

    getConsignees() {
        this.consignees = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CONSIGNEE);
    }

    getDescription(fullName: string, address: string, tel: string, fax: string) {
        return `${fullName} \n ${address} \n Tel No: ${!!tel ? tel : ''} \n Fax No: ${!!fax ? fax : ''} \n`;
    }

    getCommonData() {
        this._documentRepo.getShipmentDataCommon()
            .pipe(catchError(this.catchError))
            .subscribe(
                (commonData: any) => {
                    this.serviceTypes = this.utility.prepareNg2SelectData(commonData.serviceTypes, 'value', 'displayName');
                    this.ladingTypes = this.utility.prepareNg2SelectData(commonData.billOfLadings, 'value', 'displayName');
                    this.termTypes = this.utility.prepareNg2SelectData(commonData.freightTerms, 'value', 'displayName');
                }
            );
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'customer':
                this.customer.setValue(data.id);
                this.saleMans.forEach((item: User) => {
                    if (item.id === data.salePersonId) {
                        this.saleMan.setValue(item.id);
                    }
                });
                break;
            case 'shipper':
                this.shipper.setValue(data.id);
                this.shipperDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'consignee':
                this.consignee.setValue(data.id);
                this.consigneeDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;
            case 'notify':
                this.notifyParty.setValue(data.id);
                this.notifyPartyDescription.setValue(this.getDescription(data.partnerNameEn, data.addressEn, data.tel, data.fax));
                break;

            default:
                break;
        }

    }
}
