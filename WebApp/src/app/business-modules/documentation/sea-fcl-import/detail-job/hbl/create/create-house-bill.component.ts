import { Component, OnInit } from '@angular/core';
import { FormGroup, FormBuilder, Validators } from '@angular/forms';
import { CatalogueRepo } from 'src/app/shared/repositories';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { AppForm } from 'src/app/app.form';

@Component({
    selector: 'app-create-house-bill',
    templateUrl: './create-house-bill.component.html',
    styleUrls: ['./create-house-bill.component.scss']
})
export class CreateHouseBillComponent extends AppForm {
    formGroup: FormGroup;
    listCustomer: any = [];
    listShipper: any = [];
    listConsignee: any = [];
    listPort: any = [];
    listSupplier: any = [];
    listProvince: any = [];
    listSaleMan: any = [];
    strCustomerCurrent: any = '';
    strShipperCurrent: any = '';
    strConsigneeCurrent: any = '';
    strNotifyParty: any = '';
    strAlsoNotifyParty: any = '';
    strPortLoadingCurrent: any = '';
    strPortDischarge: any = '';
    strSupplier: any = '';
    strProvince: any = '';
    strSalemanCurrent: any = '';
    configCustomer: CommonInterface.IComboGirdConfig | any = {};

    numberOfOrigins: CommonInterface.ICommonTitleValue[] = [
        { title: 'One(1)', value: 'One' },
        { title: 'Two(2)', value: 'Two' },
        { title: 'Three(3)', value: 'Three' }

    ];
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
    ) {
        super();
    }
    ngOnInit() {
        this.initForm();
        this.configCustomer = Object.assign({}, this.configComoBoGrid, {
            displayFields: [
                { field: 'id', label: 'Partner ID' },
                { field: 'shortName', label: 'Name ABBR' },
                { field: 'partnerNameEn', label: 'Name EN' },
                { field: 'taxCode', label: 'Tax Code' },

            ],
        }, { selectedDisplayFields: ['partnerNameEn'], });
    }

    initForm() {
        this.formGroup = this._fb.group({
            masterBill: ['',
                Validators.compose([
                    Validators.required
                ])],
            hbOfladingNo: ['',
                Validators.compose([
                    Validators.required
                ])],
            hbOfladingType: ['',
                Validators.compose([
                    Validators.required
                ])],
            finalDestination: [

            ],
            placeofReceipt: ['',
                Validators.compose([
                    Validators.required
                ])],
            feederVessel1: ['',
                Validators.compose([
                    Validators.required
                ])],
            feederVessel2: ['',
                Validators.compose([
                    Validators.required
                ])],
            arrivalVessel: [
            ],
            arrivalVoyage: [
            ],
            singledater: [
            ],
            documentNo: [],
            warehousecbo: [],
            referenceNo: [],
            warehousenotice: [],
            shppingMark: [],
            remark: [],
            feederVoyageNo: [
            ],
            numberOfOrigin: [this.numberOfOrigins[0]]
        });

        this.getListCustomer();
        this.getListShipper();
        this.getListConsignee();
        this.getListPort();
        this.getListSupplier();
        this.getListProvince();
    }


    getListCustomer() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CUSTOMER })
            .subscribe((res: any) => { this.listCustomer = res; });
    }

    getListShipper() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.SHIPPER })
            .subscribe((res: any) => { this.listShipper = res; });
    }

    getListConsignee() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CONSIGNEE })
            .subscribe((res: any) => { this.listConsignee = res; });
    }

    getListSupplier() {
        this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CARRIER })
            .subscribe((res: any) => { this.listSupplier = res; });
    }

    getListPort() {
        this._catalogueRepo.getListPortByTran().subscribe((res: any) => { this.listPort = res; });
    }
    getListProvince() {
        this._catalogueRepo.getAllProvinces().subscribe((res: any) => { this.listProvince = res; });
    }
    getListSaleman() {
        this._catalogueRepo.getListSaleman(this.strCustomerCurrent.id).subscribe((res: any) => { this.listSaleMan = res; });
    }
    onSelectDataFormInfo(data: any, key: string | any) {

    }




}
