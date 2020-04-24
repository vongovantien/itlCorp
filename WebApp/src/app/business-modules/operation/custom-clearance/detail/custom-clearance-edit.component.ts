import { Component, OnInit, ViewChild } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { NgForm } from '@angular/forms';
import { CustomClearance } from 'src/app/shared/models/tool-setting/custom-clearance.model';
import { OpsTransaction } from 'src/app/shared/models/document/OpsTransaction.model';
import { PlaceTypeEnum } from 'src/app/shared/enums/placeType-enum';
import { PartnerGroupEnum } from 'src/app/shared/enums/partnerGroup.enum';
import { OperationRepo, DocumentationRepo, CatalogueRepo } from '@repositories';
import { formatDate } from '@angular/common';
import { SystemConstants } from 'src/constants/system.const';
import { ToastrService } from 'ngx-toastr';
import { AppPage } from 'src/app/app.base';

import find from 'lodash/find';
import { CustomClearanceFormDetailComponent } from '../components/form-detail-clearance/form-detail-clearance.component';
import { catchError, finalize } from 'rxjs/operators';


@Component({
    selector: 'app-custom-clearance-edit',
    templateUrl: './custom-clearance-edit.component.html',
})
export class CustomClearanceEditComponent extends AppPage implements OnInit {
    @ViewChild(CustomClearanceFormDetailComponent, { static: false }) detailComponent: CustomClearanceFormDetailComponent;
    isImported: boolean = false;
    id: number = 0;
    // selectedRange: any;
    // selectedDate: any;
    // keepCalendarOpeningWithRange: true;

    // serviceTypes: any = [];
    // typeClearance: any = [];
    // routeClearance: any = [];
    // cargoTypes: any = [];

    // strCustomerCurrent: any = '';
    // strPortCurrent: any = '';
    // strCountryImportCurrent: any = '';
    // strCountryExportCurrent: any = '';
    // strCommodityCurrent: any = '';
    // strUnitCurrent: any = '';

    // serviceTypeCurrent: any = [];
    // typeClearanceCurrent: any = [];
    // routeClearanceCurrent: any = [];
    // cargoTypeCurrent: any = [];

    //public disabled: boolean = false;


    // customDeclaration: CustomClearance = new CustomClearance();
    // listCustomer: any[] = [];
    // listPort: any = [];
    // listCountry: any = [];
    // listCommodity: any = [];
    // listUnit: any = [];
    // _clearanceDate: any;
    // isConvertJob: boolean = false;

    constructor(private _operationRepo: OperationRepo,
        private _documentation: DocumentationRepo,
        private route: ActivatedRoute,
        private toart: ToastrService,
        private _catalogueRepo: CatalogueRepo) {

        super();
        // this.keepCalendarOpeningWithRange = true;
        // this.selectedDate = Date.now();
        // this.selectedRange = { startDate: this.createMoment().startOf('month'), endDate: this.createMoment().endOf('month') };

    }

    ngOnInit() {
        this.route.params.subscribe(prams => {
            if (!!prams.id) {
                this.id = prams.id;
                this.getCustomCleanranceById(+prams.id);
            }
        });
        this._operationRepo.getClearanceTypes()
            .subscribe((res: any) => {
                if (!!res) {

                    this.detailComponent.serviceTypes = res.serviceTypes.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.typeClearances = res.types.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.routeClearances = res.routes.map(x => ({ "text": x.displayName, "id": x.value }));
                    this.detailComponent.cargoTypes = res.cargoTypes.map(x => ({ "text": x.displayName, "id": x.value }));
                }
            });
    }

    getCustomCleanranceById(id: number) {
        this._operationRepo.getDetailCustomsDeclaration(id)
            .pipe()
            .subscribe(
                (res: CustomClearance) => {
                    if (!!res) {
                        if (!!res.jobNo) { this.isImported = true; }
                        this.detailComponent.customDeclaration = res;
                        this.detailComponent.setFormValue();
                    }
                }
            );
    }
    updateSuccess(event) {
        if (event) {
            this.isImported = true;
            this.getCustomCleanranceById(this.id);
        }
    }

    // async updateCustomClearance(formUpdate: NgForm) {
    //     if (this.strCustomerCurrent === '' || this.strPortCurrent === '') { return; }
    //     if (this.serviceTypeCurrent[0] !== 'Air' && this.serviceTypeCurrent[0] !== 'Express') {
    //         if (this.cargoTypeCurrent.length === 0) { return; }
    //     }
    //     if (formUpdate.form.status !== "INVALID" && this._clearanceDate.endDate != null) {
    //         this.customDeclaration.partnerTaxCode = this.strCustomerCurrent;
    //         this.customDeclaration.clearanceDate = formatDate(this._clearanceDate.endDate, 'yyyy-MM-dd', 'en');
    //         this.customDeclaration.serviceType = this.serviceTypeCurrent[0];
    //         this.customDeclaration.gateway = this.strPortCurrent;
    //         this.customDeclaration.type = this.typeClearanceCurrent[0];
    //         this.customDeclaration.route = this.routeClearanceCurrent[0];
    //         this.customDeclaration.cargoType = (this.serviceTypeCurrent[0] === 'Air' || this.serviceTypeCurrent[0] === 'Express') ? null : this.cargoTypeCurrent[0];
    //         this.customDeclaration.exportCountryCode = this.strCountryExportCurrent;
    //         this.customDeclaration.importCountryCode = this.strCountryImportCurrent;
    //         this.customDeclaration.commodityCode = this.strCommodityCurrent;
    //         this.customDeclaration.unitCode = this.strUnitCurrent;

    //         const respone = await this._operationRepo.updateCustomDeclaration(this.customDeclaration).toPromise();
    //         if (respone['status'] === true) {
    //             this.toart.success(respone['message']);
    //             this.getCustomCleanranceById(this.customDeclaration.id);
    //             //this.mapClearanceToShipment();
    //         } else {
    //             this._clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: new Date(this.customDeclaration.clearanceDate), endDate: new Date(this.customDeclaration.clearanceDate) };
    //         }
    //     }
    // }
    // async convertClearanceToShipment(formUpdate: NgForm) {
    //     if (this.strCustomerCurrent === '' || this.strPortCurrent === ''
    //         || this.typeClearanceCurrent.length === 0
    //         || this.customDeclaration.hblid == null || this.customDeclaration.hblid === ''
    //         || this.customDeclaration.mblid == null || this.customDeclaration.mblid === '') { return; }
    //     if (this.serviceTypeCurrent[0] !== 'Sea' && this.cargoTypeCurrent.length === 0) {
    //         return;
    //     }
    //     if (this.serviceTypeCurrent[0] !== 'Air' && this.serviceTypeCurrent[0] !== 'Express') {
    //         if (this.cargoTypeCurrent.length === 0) { return; }
    //     }
    //     if (formUpdate.form.status !== "INVALID" && this._clearanceDate.endDate != null) {
    //         this.customDeclaration.partnerTaxCode = this.strCustomerCurrent;
    //         this.customDeclaration.clearanceDate = formatDate(this._clearanceDate.endDate, "yyyy-MM-dd", "en");
    //         this.customDeclaration.serviceType = this.serviceTypeCurrent[0];
    //         this.customDeclaration.gateway = this.strPortCurrent;
    //         this.customDeclaration.type = this.typeClearanceCurrent[0];
    //         this.customDeclaration.route = this.routeClearanceCurrent[0];
    //         this.customDeclaration.cargoType = (this.serviceTypeCurrent[0] === 'Air' || this.serviceTypeCurrent[0] === 'Express') ? null : this.cargoTypeCurrent[0];
    //         this.customDeclaration.exportCountryCode = this.strCountryExportCurrent;
    //         this.customDeclaration.importCountryCode = this.strCountryImportCurrent;
    //         this.customDeclaration.commodityCode = this.strCommodityCurrent;
    //         this.customDeclaration.unitCode = this.strUnitCurrent;

    //         //const shipment = this.mapClearanceToShipment();
    //         // const response = await this._documentation.convertExistedClearanceToJob([{ opsTransaction: shipment, customsDeclaration: this.customDeclaration }]).toPromise();
    //         // if (response.status) {
    //         //     this.getCustomCleanranceById(this.customDeclaration.id);
    //         // } else {
    //         //     // reset lại _clearanceDate
    //         //     this._clearanceDate = this.customDeclaration.clearanceDate == null ? this.customDeclaration.clearanceDate : { startDate: new Date(this.customDeclaration.clearanceDate.startDate), endDate: new Date(this.customDeclaration.clearanceDate.startDate) };
    //         // }
    //     }
    // }

    // mapClearanceToShipment() {
    //     let shipment = new OpsTransaction();
    //     let index = this.listCustomer.findIndex(x => x.taxCode === this.customDeclaration.partnerTaxCode);
    //     if (index > -1) {
    //         const customer = this.listCustomer[index];
    //         shipment.customerId = customer.id;
    //         shipment.salemanId = customer.salePersonId;
    //         shipment.serviceMode = this.customDeclaration.type;
    //         index = this.listPort.findIndex(x => x.code === this.customDeclaration.gateway);
    //         if (index > -1) {
    //             if (this.customDeclaration.type === "Export") {
    //                 shipment.pol = this.listPort[index].id;
    //                 shipment.clearanceLocation = shipment.pol;
    //             }
    //             if (this.customDeclaration.type === "Import") {
    //                 shipment.pod = this.listPort[index].id;
    //                 shipment.clearanceLocation = shipment.pod;
    //             }
    //         }
    //         if (this.customDeclaration.serviceType === "Sea") {
    //             if (this.customDeclaration.cargoType === "FCL") {
    //                 shipment.productService = "SeaFCL";
    //             }
    //             if (this.customDeclaration.cargoType === "LCL") {
    //                 shipment.productService = "SeaLCL";
    //             }
    //         } else {
    //             shipment.productService = this.customDeclaration.serviceType;
    //         }
    //         shipment.shipmentMode = "External";
    //         shipment.mblno = this.customDeclaration.mblid;
    //         shipment.hwbno = this.customDeclaration.hblid;
    //         shipment.serviceDate = this.customDeclaration.clearanceDate;
    //         shipment.sumGrossWeight = this.customDeclaration.grossWeight;
    //         shipment.sumNetWeight = this.customDeclaration.netWeight;
    //         shipment.sumCbm = this.customDeclaration.cbm;

    //         shipment.shipper = this.customDeclaration.shipper;
    //         shipment.consignee = this.customDeclaration.consignee;
    //         const claim = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
    //         const currenctUser = claim["id"];
    //         shipment.billingOpsId = currenctUser;
    //         index = this.listUnit.findIndex(x => x.code === this.customDeclaration.unitCode);
    //         if (index > -1) {
    //             shipment.packageTypeId = this.listUnit[index].id;
    //         }
    //     } else {
    //         this.toart.error("Không có customer để tạo job mới");
    //         shipment = null;
    //     }
    //     if (this.customDeclaration.clearanceDate == null) {
    //         this.toart.error("Không có clearance date để tạo job mới");
    //         shipment = null;
    //     }
    //     return shipment;
    // }

    // async getListCustomer() {
    //     this.listCustomer = await this._catalogueRepo.getListPartner(null, null, { partnerGroup: PartnerGroupEnum.CUSTOMER }).toPromise();
    // }
    // async getListPort() {
    //     this.listPort = await this._catalogueRepo.getPlace({ placeType: PlaceTypeEnum.Port }).toPromise();
    // }

    // getListCountry() {
    //     this._catalogueRepo.getCountry().subscribe((res: any) => { this.listCountry = res; });
    // }

    // getListCommodity() {
    //     this._catalogueRepo.getCommondity({}).subscribe((res: any) => { this.listCommodity = res; });
    // }

    // getListUnit() {
    //     // unitType = Package
    //     this._catalogueRepo.getUnit({ unitType: 'Package' }).subscribe((res: any) => { this.listUnit = res; });
    // }



    // public selectedServiceType(value: any): void {
    //     this.serviceTypeCurrent = [value.id];
    //     if (this.serviceTypeCurrent[0] == 'Air' || this.serviceTypeCurrent[0] == 'Express') {
    //         this.cargoTypeCurrent = [];
    //     }
    // }

    // public selectedTypeClearance(value: any): void {
    //     this.typeClearanceCurrent = [value.id];
    // }

    // public selectedRouteClearance(value: any): void {
    //     this.routeClearanceCurrent = [value.id];
    // }

    // public selectedCargoType(value: any): void {
    //     this.cargoTypeCurrent = [value.id];
    // }

    // public removedServiceType(): void {
    //     this.serviceTypeCurrent = [];
    // }

    // public removedTypeClearance(): void {
    //     this.typeClearanceCurrent = [];
    // }

    // public removedRouteClearance(): void {
    //     this.routeClearanceCurrent = [];
    // }

    // public removedCargoType(): void {
    //     this.cargoTypeCurrent = [];
    // }
}
