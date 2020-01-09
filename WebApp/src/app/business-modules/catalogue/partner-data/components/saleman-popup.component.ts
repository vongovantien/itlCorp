import { Component, Output, EventEmitter, Input } from '@angular/core';
import { BaseService } from 'src/app/shared/services/base.service';
import { API_MENU } from 'src/constants/api-menu.const';
import { CatalogueRepo, SystemRepo } from 'src/app/shared/repositories';
import { catchError } from "rxjs/operators";
import { Saleman } from 'src/app/shared/models/catalogue/saleman.model';
import { PopupBase } from 'src/app/popup.base';
import { formatDate } from '@angular/common';
import { FormGroup, FormBuilder, AbstractControl, Validators } from '@angular/forms';

@Component({
    selector: 'app-saleman-popup',
    templateUrl: './saleman-popup.component.html',
})

export class SalemanPopupComponent extends PopupBase {
    @Output() onCreateToForm = new EventEmitter();
    @Output() onDelete = new EventEmitter();
    saleMans: Saleman[] = [];
    headerSaleman: CommonInterface.IHeaderTable[];
    services: any[] = [];
    statuss: any[] = [];
    offices: any[] = [];
    selectedService: any = {};
    saleManToAdd: Saleman = new Saleman();
    strSalemanCurrent: any = {};
    strOfficeCurrent: any = '';
    selectedStatus: any = {};
    users: any[] = [];
    isSave: boolean = false;
    isExistedSaleman: boolean = false;
    isDup: boolean = false;
    saleManToView: Saleman = new Saleman();
    isDetail: boolean = false;
    selectedDataSaleMan: any;
    selectedDataOffice: any;
    index: number = 0;
    currrently_user: string = '';
    form: FormGroup;
    saleman: AbstractControl;
    service: AbstractControl;
    office: AbstractControl;
    status: AbstractControl;
    effectiveDate: AbstractControl;
    description: AbstractControl;
    freightPayment: AbstractControl;
    termTypes: CommonInterface.INg2Select[];

    @Input() popupData: Saleman;
    constructor(
        private baseService: BaseService,
        private api_menu: API_MENU,
        private _catalogueRepo: CatalogueRepo,
        private _systemRepo: SystemRepo,
        private _fb: FormBuilder
    ) {

        super();
    }

    ngOnInit() {
        this.termTypes = [
            { id: 'All', text: 'All' },
            { id: 'Prepaid', text: 'Prepaid' },
            { id: 'Collect', text: 'Collect' },
        ];
        this.getComboboxData();
        this.currrently_user = localStorage.getItem('currently_userName');
        this.form = this._fb.group({
            saleman: [null, Validators.required],
            office: [null, Validators.required],
            status: [false],
            effectiveDate: [],
            description: [],
            service: [this.services[0], Validators.required],
            freightPayment: [this.termTypes[0]]

        });

        this.saleman = this.form.controls["saleman"];
        this.service = this.form.controls["service"];
        this.office = this.form.controls["office"];
        this.status = this.form.controls["status"];
        this.effectiveDate = this.form.controls["effectiveDate"];
        this.description = this.form.controls["description"];
        this.freightPayment = this.form.controls["freightPayment"];
        this.freightPayment.setValue([<CommonInterface.INg2Select>{ id: "All", text: "All" }]);
    }

    showSaleman(sm: Saleman) {
        this.form.setValue({
            office: sm.office,
            effectiveDate: !!sm.effectDate ? { startDate: new Date(sm.effectDate), endDate: new Date(sm.effectDate) } : null,
            service: [<CommonInterface.INg2Select>{ id: sm.service, text: sm.serviceName }],
            status: sm.status,
            saleman: sm.saleManId,
            description: sm.description,
            freightPayment: [<CommonInterface.INg2Select>{ id: sm.freightPayment, text: sm.freightPayment }]
        });
    }

    onDeleteSaleman() {
        this.isDetail = false;
        this.onDelete.emit(this.index);
        this.hide();
    }

    resetForm() {
        this.saleman.setValue(null);
        this.office.setValue(null);
        this.freightPayment.setValue([<CommonInterface.INg2Select>{ id: "All", text: "All" }]);
        this.service.setValue(null);
        this.effectiveDate.setValue(null);
        this.description.setValue(null);
        this.status.setValue(false);

    }
    closePoup() {
        this.isDetail = false;
        this.hide();

    }

    getComboboxData(): any {
        this.getSalemans();
        this.getService();
        this.getOffice();
        this.statuss = this.getStatus();

    }

    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.services = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        // this.service.setValue(this.services.filter(i => i.value === res.value)[1]);
                    }
                },
            );
    }

    applyToList() {
        this.isSave = true;
        this.onCreate();
    }

    getOffice() {
        this._catalogueRepo.getListBranch()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.offices = res;
                        console.log(this.offices);
                    }
                },
            );
    }

    getStatus(): CommonInterface.ICommonTitleValue[] {
        return [
            { title: 'Active', value: true },
            { title: 'Inactive', value: false },
        ];
    }

    getSalemans() {
        this._systemRepo.getListSystemUser({})
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.users = res;
                    }
                },
            );
    }

    onSelectSaleMan(saleMan: any) {
        this.saleman.setValue(saleMan.id);
        this.selectedDataSaleMan = saleMan;
        console.log(this.selectedDataSaleMan);
    }

    onSelectOffice(office: any) {
        this.office.setValue(office.abbrCompany);
        this.selectedDataOffice = office;
    }

    onCreate() {
        this.isSave = true;
        this.setError(this.freightPayment);
        if (this.form.valid) {
            const salemans: any = {
                company: this.office.value,
                office: this.office.value,
                effectDate: !!this.effectiveDate.value && !!this.effectiveDate.value.startDate ? formatDate(this.effectiveDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
                status: this.status.value,
                partnerId: null,
                saleManId: this.saleman.value,
                service: this.service.value[0].id,
                createDate: new Date(),
                username: this.selectedDataSaleMan.username,
                freightPayment: this.freightPayment.value[0].id,
                description: this.description.value,
                serviceName: this.service.value[0].text,
            };
            console.log(salemans);
            this.onCreateToForm.emit(new Saleman(salemans));
        }
    }
}
