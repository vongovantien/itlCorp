import { Component } from "@angular/core";
import { AppForm } from "src/app/app.form";
import { FormBuilder, FormGroup, AbstractControl } from "@angular/forms";
import { Observable } from "rxjs";
import { Customer, PortIndex } from "@models";
import { CommonEnum } from "@enums";
import { CatalogueRepo, SystemRepo } from "@repositories";
import { catchError, takeUntil, finalize } from "rxjs/operators";
import { formatDate } from "@angular/common";
import { SystemConstants } from "src/constants/system.const";
import { Store } from "@ngrx/store";
import { IAppState, getMenuUserPermissionState } from "@store";

@Component({
    selector: 'general-report-form-search',
    templateUrl: './form-search-general-report.component.html'
})

export class GeneralReportFormSearchComponent extends AppForm {
    menuPermission: SystemInterface.IUserPermission;

    formSearch: FormGroup;
    serviceDate: AbstractControl;
    dateType: AbstractControl;
    customer: AbstractControl;
    carrier: AbstractControl;
    agent: AbstractControl;
    service: AbstractControl;
    currency: AbstractControl;
    refNo: AbstractControl;
    refNoType: AbstractControl;
    office: AbstractControl;
    department: AbstractControl;
    group: AbstractControl;
    staff: AbstractControl;
    staffType: AbstractControl;
    pol: AbstractControl;
    pod: AbstractControl;

    displayFieldsCustomer: CommonInterface.IComboGridDisplayField[] = [
        { field: 'taxCode', label: 'Tax Code' },
        { field: 'shortName', label: 'Name ABBR' }
    ];

    displayFieldPort: CommonInterface.IComboGridDisplayField[] = [
        { field: 'code', label: 'Port Code' },
        { field: 'nameEn', label: 'Port Name' }
    ];

    customers: Observable<Customer[]>;
    carriers: Observable<Customer[]>;
    agents: Observable<Customer[]>;
    ports: Observable<PortIndex[]>;

    dateTypeList: any[] = [];
    dateTypeActive: any[] = [];
    serviceList: any[] = [];
    serviceActive: any[] = [];
    currencyList: any[] = [];
    currencyActive: any[] = [];
    refNoTypeList: any[] = [];
    refNoTypeActive: any[] = [];
    officeList: any[] = [];
    officeActive: any[] = [];
    departmentList: any[] = [];
    departmentActive: any[] = [];
    groupList: any[] = [];
    groupActive: any[] = [];
    staffList: any[] = [];
    staffActive: any[] = [];
    staffTypeList: any[] = [];
    staffTypeActive: any[] = [];

    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<IAppState>,
        private _systemRepo: SystemRepo,
    ) {
        super();
    }

    ngOnInit() {
        this._store.select(getMenuUserPermissionState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res: SystemInterface.IUserPermission) => {
                if (res !== null && res !== undefined) {
                    this.menuPermission = res;
                    console.log(this.menuPermission);
                }
            });

        this.initDataInform();
        this.initFormSearch();
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER, null);
        this.agents = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.AGENT, null);
        this.carriers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CARRIER, null);
        this.ports = this._catalogueRepo.getListPort({ placeType: CommonEnum.PlaceTypeEnum.Port });
    }

    initFormSearch() {
        this.formSearch = this._fb.group({
            serviceDate: [{
                startDate: this.createMoment().startOf('month').toDate(),
                endDate: this.createMoment().endOf('month').toDate(),
            }],
            dateType: [this.dateTypeActive],
            customer: [],
            carrier: [],
            agent: [],
            service: [this.serviceActive],
            currency: [this.currencyActive],
            refNo: [],
            refNoType: [this.refNoTypeActive],
            office: [],
            department: [],
            group: [],
            staff: [],
            staffType: [this.staffTypeActive],
            pol: [],
            pod: [],
        });

        this.serviceDate = this.formSearch.controls['serviceDate'];
        this.dateType = this.formSearch.controls['dateType'];
        this.customer = this.formSearch.controls['customer'];
        this.carrier = this.formSearch.controls['carrier'];
        this.agent = this.formSearch.controls['agent'];
        this.service = this.formSearch.controls['service'];
        this.currency = this.formSearch.controls['currency'];
        this.refNo = this.formSearch.controls['refNo'];
        this.refNoType = this.formSearch.controls['refNoType'];
        this.office = this.formSearch.controls['office'];
        this.department = this.formSearch.controls['department'];
        this.group = this.formSearch.controls['group'];
        this.staff = this.formSearch.controls['staff'];
        this.staffType = this.formSearch.controls['staffType'];
        this.pol = this.formSearch.controls['pol'];
        this.pod = this.formSearch.controls['pod'];

    }

    initDataInform() {
        this.getDateType();
        this.getService();
        this.getCurrency();
        this.getRefNoType();
        this.getOffice();
        this.getDepartment();
        this.getGroup();
        this.getStaffType();
    }

    onSelectDataFormInfo(data: any, type: string) {
        switch (type) {
            case 'customer':
                this.customer.setValue(data.id);
                break;
            case 'carrier':
                this.carrier.setValue(data.id);
                break;
            case 'agent':
                this.agent.setValue(data.id);
                break;
            case 'pol':
                this.pol.setValue(data.id);
                break;
            case 'pod':
                this.pod.setValue(data.id);
                break;
            case 'service':
                if (data.id === 'All') {
                    this.serviceActive = [];
                    this.serviceActive.push({ id: 'All', text: "All" });
                } else {
                    this.serviceActive.push(data);
                    this.detectServiceWithAllOption(data);
                    this.service.setValue(this.serviceActive);
                }
                break;
            case 'currency':
                this.currencyActive = [];
                this.currencyActive.push(data);
                this.currency.setValue(this.currencyActive);
                break;
            default:
                break;
        }
    }

    detectServiceWithAllOption(data?: any) {
        if (!this.serviceActive.every((value: any) => value.id !== 'All')) {
            this.serviceActive.splice(this.serviceActive.findIndex((item: any) => item.id === 'All'), 1);
            this.serviceActive = [];
            this.serviceActive.push(data);
        }
    }

    getDateType() {
        this.dateTypeList = [
            { "text": 'Service Date', "id": 'ServiceDate' },
            { "text": 'Created Date', "id": 'CreatedDate' }
        ];
        // Default value: Service Date
        this.dateTypeActive = [this.dateTypeList[0]];
    }

    getService() {
        this._catalogueRepo.getListService()
            .pipe(catchError(this.catchError))
            .subscribe(
                (res: any) => {
                    if (!!res) {
                        this.serviceList = this.utility.prepareNg2SelectData(res, 'value', 'displayName');
                        this.serviceList.unshift({ id: 'All', text: 'All' });
                        this.serviceActive = [this.serviceList[0]];
                    }
                },
            );
    }

    getCurrency() {
        this._catalogueRepo.getListCurrency()
            .pipe(catchError(this.catchError))
            .subscribe(
                (data: any) => {
                    if (!!data) {
                        this.currencyList = data.map((item: any) => ({ text: item.id, id: item.id }));
                        // Default value: USD
                        this.currencyActive = [this.currencyList.filter((curr) => curr.id === "USD")[0]];
                    }
                },
            );
    }

    getRefNoType() {
        this.refNoTypeList = [
            { "text": 'JOB JD', "id": 'JOBID' },
            { "text": 'HBL/HAWB', "id": 'HBL' },
            { "text": 'MBL/MAWB', "id": 'MBL' }
        ];
        // Default value: JOB ID
        this.refNoTypeActive = [this.refNoTypeList[0]];
    }

    getOffice() {
        const userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this._systemRepo.getListUserLevelByUserId(userLogged.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe(
                (res: any) => {
                    if (!!res) {
                        console.log(res);
                    }
                },
            );
    }

    getDepartment() {

    }

    getGroup() {

    }

    getStaff() {

    }

    getStaffType() {
        this.staffTypeList = [
            { "text": 'Person In Charge', "id": 'PIC' },
            { "text": 'Salesman', "id": 'SALESMAN' },
            { "text": 'Creator', "id": 'CREATOR' }
        ];
        // Default value: Person In Charge
        this.staffTypeActive = [this.staffTypeList[0]];
    }

    searchReport() {
        const body: ISearchDataCriteria = {
            serviceDateFrom: this.dateType.value[0].id === "ServiceDate" ? formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            serviceDateTo: this.dateType.value[0].id === "ServiceDate" ? formatDate(this.serviceDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            createdDateFrom: this.dateType.value[0].id === "CreatedDate" ? formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            createdDateTo: this.dateType.value[0].id === "CreatedDate" ? formatDate(this.serviceDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            customerId: this.customer.value,
            service: this.serviceActive.map((item: any) => item.id).toString().replace(/(?:,)/g, ';'), // ---*
            currency: this.currencyActive[0].id, // ---**
            jobId: this.refNoType.value[0].id === "JOBID" ? this.refNo.value : null,
            mawb: this.refNoType.value[0].id === "MBL" ? this.refNo.value : null,
            hawb: this.refNoType.value[0].id === "HBL" ? this.refNo.value : null,
            officeId: this.office.value,
            departmentId: this.department.value,
            groupId: this.group.value,
            personInCharge: this.staffType.value[0].id === "PIC" ? this.staff.value : null,
            salesMan: this.staffType.value[0].id === "SALESMAN" ? this.staff.value : null,
            creator: this.staffType.value[0].id === "CREATOR" ? this.staff.value : null,
            carrierId: this.carrier.value,
            agentId: this.agent.value,
            pol: this.pol.value,
            pod: this.pod.value,
        };
        console.log(body);
    }

    resetSearch() {
        this.formSearch.reset();
        this.initDataInform();
        this.resetFormControl(this.customer);
        this.resetFormControl(this.carrier);
        this.resetFormControl(this.agent);
        this.resetFormControl(this.pol);
        this.resetFormControl(this.pod);
        // this.onSearch.emit(<any>{});
        this.dateTypeActive = [this.dateTypeList[0]];
        this.dateType.setValue(this.dateTypeActive);

        this.serviceActive = [this.serviceList[0]];
        this.service.setValue(this.serviceActive);

        this.currencyActive = [this.currencyList[0]];
        this.currency.setValue(this.currencyActive);

        this.refNoTypeActive = [this.refNoTypeList[0]];
        this.refNoType.setValue(this.refNoTypeActive);

        this.staffTypeActive = [this.staffTypeList[0]];
        this.staffType.setValue(this.staffTypeActive);

        this.serviceDate.setValue({
            startDate: this.createMoment().startOf('month').toDate(),
            endDate: this.createMoment().endOf('month').toDate(),
        });
    }

}

interface ISearchDataCriteria {
    serviceDateFrom: string;
    serviceDateTo: string;
    createdDateFrom: string;
    createdDateTo: string;
    customerId: string;
    service: string;
    currency: string;
    jobId: string;
    mawb: string;
    hawb: string;
    officeId: string;
    departmentId: string;
    groupId: string;
    personInCharge: string;
    salesMan: string;
    creator: string;
    carrierId: string;
    agentId: string;
    pol: string;
    pod: string;
}