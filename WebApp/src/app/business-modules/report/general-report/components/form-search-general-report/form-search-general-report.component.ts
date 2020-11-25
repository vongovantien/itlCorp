import { Component, EventEmitter, Output } from "@angular/core";
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
import { ReportInterface } from "@interfaces";

@Component({
    selector: 'general-report-form-search',
    templateUrl: './form-search-general-report.component.html'
})

export class GeneralReportFormSearchComponent extends AppForm {
    @Output() onSearch: EventEmitter<ReportInterface.ISearchDataCriteria> = new EventEmitter<ReportInterface.ISearchDataCriteria>();

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

    userLogged: any;

    officesInit: any[] = [];
    departmentsInit: any[] = [];
    groupsInit: any[] = [];
    staffsInit: any[] = [];

    groupSpecial: any[] = [];
    constructor(
        private _fb: FormBuilder,
        private _catalogueRepo: CatalogueRepo,
        private _store: Store<IAppState>,
        private _systemRepo: SystemRepo,
    ) {
        super();
    }

    ngOnInit() {
        this.initDataInform();
        this.initFormSearch();
        this.customers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CUSTOMER, null);
        this.agents = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.AGENT, null);
        this.carriers = this._catalogueRepo.getPartnersByType(CommonEnum.PartnerGroupEnum.CARRIER, null);
        this.ports = this._catalogueRepo.getListPort({ placeType: CommonEnum.PlaceTypeEnum.Port });

        this.userLogged = JSON.parse(localStorage.getItem(SystemConstants.USER_CLAIMS));
        this._store.select(getMenuUserPermissionState)
            .pipe(takeUntil(this.ngUnsubscribe))
            .subscribe((res: SystemInterface.IUserPermission) => {
                if (res !== null && res !== undefined && Object.keys(res).length !== 0) {
                    this.menuPermission = res;
                    if (this.menuPermission.list !== 'None') {
                        if (this.menuPermission.list === 'All' || this.menuPermission.list === 'Company') {
                            this.getAllOffice();
                            this.getAllDepartment();
                            this.getAllGroup();
                        } else if (this.menuPermission.list === 'All' || this.menuPermission.list === 'Company' || this.menuPermission.list === 'Office') {
                            this.getAllDepartment();
                            this.getAllGroup();
                        } else if (this.menuPermission.list === 'All' || this.menuPermission.list === 'Company' || this.menuPermission.list === 'Office' || this.menuPermission.list === 'Department') {
                            this.getAllGroup();
                        }
                        this.getDataUserLever();
                        this.getAllStaff();
                    }
                }
            });
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
            office: [this.officeActive],
            department: [this.departmentActive],
            group: [this.groupActive],
            staff: [this.staffActive],
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
                    this.detectServiceWithAllOption('service', data);
                    this.service.setValue(this.serviceActive);
                }
                break;
            case 'office':
                if (data.id === 'All') {
                    this.officeActive = [];
                    this.officeActive.push({ id: 'All', text: "All" });
                } else {
                    this.officeActive.push(data);
                    this.detectServiceWithAllOption('office', data);
                    this.office.setValue(this.officeActive);
                }
                // Reload deparment
                this.getDepartment(this.departmentsInit);
                break;
            case 'department':
                if (data.id === 'All') {
                    this.departmentActive = [];
                    this.departmentActive.push({ id: 'All', text: "All" });
                } else {
                    this.departmentActive.push(data);
                    this.detectServiceWithAllOption('department', data);
                    this.department.setValue(this.departmentActive);
                }
                // Reload group
                this.getGroup(this.groupsInit);
                break;
            case 'group':
                if (data.id === 'All') {
                    this.groupActive = [];
                    this.groupActive.push({ id: 'All', text: "All" });
                } else {
                    this.groupActive.push(data);
                    this.detectServiceWithAllOption('group', data);
                    this.group.setValue(this.groupActive);
                }
                // Reload staff
                this.getStaff(this.staffsInit);
                break;
            case 'staff':
                if (data.id === 'All') {
                    this.staffActive = [];
                    this.staffActive.push({ id: 'All', text: "All" });
                } else {
                    this.staffActive.push(data);
                    this.detectServiceWithAllOption('staff', data);
                    this.staff.setValue(this.staffActive);
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

    onRemoveDataFormInfo(data: any, type: string) {
        if (type === 'service') {
            this.serviceActive.splice(this.serviceActive.findIndex((item) => item.id === data.id), 1);
        }
        if (type === 'office') {
            this.officeActive.splice(this.officeActive.findIndex((item) => item.id === data.id), 1);
            if (this.officeActive.length === 0) {
                this.department.setValue([]);
                this.departmentActive = [];
                this.group.setValue([]);
                this.groupActive = [];
                this.staff.setValue([]);
                this.staffActive = [];
            }
        }
        if (type === 'department') {
            this.departmentActive.splice(this.departmentActive.findIndex((item) => item.id === data.id), 1);
            if (this.departmentActive.length === 0) {
                this.group.setValue([]);
                this.groupActive = [];
                this.staff.setValue([]);
                this.staffActive = [];
            }
        }
        if (type === 'group') {
            this.groupActive.splice(this.groupActive.findIndex((item) => item.id === data.id), 1);
            if (this.groupActive.length === 0) {
                this.staff.setValue([]);
                this.staffActive = [];
            }
        }
        if (type === 'staff') {
            this.staffActive.splice(this.staffActive.findIndex((item) => item.id === data.id), 1);
        }
        this.detectServiceWithAllOption(type);
    }

    detectServiceWithAllOption(type: string, data?: any) {
        if (type === "service") {
            if (!this.serviceActive.every((value) => value.id !== 'All')) {
                this.serviceActive.splice(this.serviceActive.findIndex((item) => item.id === 'All'), 1);
                this.serviceActive = [];
                this.serviceActive.push(data);
            }
        } else if (type === 'office') {
            if (!this.officeActive.every((value) => value.id !== 'All')) {
                this.officeActive.splice(this.officeActive.findIndex((item) => item.id === 'All'), 1);
                this.officeActive = [];
                this.officeActive.push(data);
            }
            // Call back Get Department            
            this.getDepartment(this.departmentsInit);
        } else if (type === 'department') {
            if (!this.departmentActive.every((value) => value.id !== 'All')) {
                this.departmentActive.splice(this.departmentActive.findIndex((item) => item.id === 'All'), 1);
                this.departmentActive = [];
                this.departmentActive.push(data);
            }
            // Call back Get Group
            this.getGroup(this.groupsInit);
        } else if (type === 'group') {
            if (!this.groupActive.every((value) => value.id !== 'All')) {
                this.groupActive.splice(this.groupActive.findIndex((item) => item.id === 'All'), 1);
                this.groupActive = [];
                this.groupActive.push(data);
            }
            // Call back Get Staff
            this.getStaff(this.staffsInit);
        } else if (type === 'staff') {
            if (!this.staffActive.every((value) => value.id !== 'All')) {
                this.staffActive.splice(this.staffActive.findIndex((item) => item.id === 'All'), 1);
                this.staffActive = [];
                this.staffActive.push(data);
            }
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
                        this.currencyList = data.map((item) => ({ text: item.id, id: item.id }));
                        // Default value: USD
                        this.currencyActive = [this.currencyList.filter((curr) => curr.id === "USD")[0]];
                    }
                },
            );
    }

    getRefNoType() {
        this.refNoTypeList = [
            { text: 'JOB JD', id: 'JOBID' },
            { text: 'HBL/HAWB', id: 'HBL' },
            { text: 'MBL/MAWB', id: 'MBL' }
        ];
        // Default value: JOB ID
        this.refNoTypeActive = [this.refNoTypeList[0]];
    }

    getDataUserLever() {
        this._systemRepo.getListUserLevelByUserId(this.userLogged.id)
            .pipe(
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe(
                (res: any) => {
                    if (!!res) {
                        if (this.menuPermission.list !== 'All' && this.menuPermission.list !== 'Company') {
                            const office = res.map((item: any) => ({ companyId: item.companyId, officeId: item.officeId, officeAbbrName: item.officeAbbrName }));
                            this.officesInit = office;
                            this.getOffice(office);
                        }

                        if (this.menuPermission.list !== 'All' && this.menuPermission.list !== 'Company' && this.menuPermission.list !== 'Office') {
                            const department = res.map((item: any) => ({ officeId: item.officeId, departmentId: item.departmentId, departmentAbbrName: item.departmentAbbrName }));
                            this.departmentsInit = department;
                            this.getDepartment(department);
                        }

                        if (this.menuPermission.list !== 'All' && this.menuPermission.list !== 'Company' && this.menuPermission.list !== 'Office' && this.menuPermission.list !== 'Department') {
                            const group = res.map((item: any) => ({ departmentId: item.departmentId, groupId: item.groupId, groupAbbrName: item.groupAbbrName }));
                            this.groupsInit = group;
                            this.getGroup(group);
                        }
                    }
                },
            );
    }

    getOffice(data: any) {
        if (this.menuPermission.list === 'All') {
            data = data;
        } else if (this.menuPermission.list === 'Company') {
            data = data.filter(f => f.officeId !== null && f.companyId === this.userLogged.companyId);
        } else {
            data = data.filter(f => f.officeId !== null && f.officeId === this.userLogged.officeId);
        }

        this.officeList = data
            .map((item: any) => ({ text: item.officeAbbrName, id: item.officeId }))
            .filter((o, i, arr) => arr.findIndex(t => t.id === o.id) === i); // Distinct office

        if (this.officeList.length > 0) {
            this.officeList.unshift({ id: 'All', text: 'All' });
            if (this.menuPermission.list === 'Office'
                || this.menuPermission.list === 'Company'
                || this.menuPermission.list === 'All') {
                this.officeActive = [this.officeList[0]];
            } else {
                this.officeActive = [this.officeList.filter(off => off.id === this.userLogged.officeId)[0]];
            }
        } else {
            this.office.setValue([]);
            this.officeActive = [];
        }

        this.getDepartment(this.departmentsInit);
    }

    getDepartment(data: any) {
        let officeSelected = [];
        if (this.officeActive.length > 0) {
            if (this.officeActive.map(i => i.id).includes('All')) {
                officeSelected = this.officeList.map(i => i.id);
            } else {
                officeSelected = this.officeActive.map(i => i.id);
            }
            data = data.filter(f => f.departmentId !== null && officeSelected.includes(f.officeId));
        } else {
            data = [];
        }

        this.departmentList = data
            .map((item: any) => ({ text: item.departmentAbbrName, id: item.departmentId.toString() }))
            .filter((d, i, arr) => arr.findIndex(t => t.id === d.id) === i); // Distinct department

        if (this.departmentList.length > 0 && this.officeActive.length > 0) {
            this.departmentList.unshift({ id: 'All', text: 'All' });
            if (this.menuPermission.list === 'Department'
                || this.menuPermission.list === 'Office'
                || this.menuPermission.list === 'Company'
                || this.menuPermission.list === 'All') {
                this.departmentActive = [this.departmentList[0]];
            } else {
                this.departmentActive = [this.departmentList.filter(dept => dept.id === this.userLogged.departmentId)[0]];
            }
        } else {
            this.department.setValue([]);
            this.departmentActive = [];
        }

        this.getGroup(this.groupsInit);
    }

    getGroup(data: any) {
        let deparmentSelected = [];
        if (this.departmentActive.length > 0) {
            if (this.departmentActive.map(i => i.id).includes('All')) {
                deparmentSelected = this.departmentList.map(i => i.id);
            } else {
                deparmentSelected = this.departmentActive.map(i => i.id);
            }
            data = data.filter(f => f.groupId !== null && f.departmentId !== null && deparmentSelected.includes(f.departmentId.toString()));
        } else {
            data = [];
        }

        this.groupList = data
            .map((item: any) => ({ text: item.groupAbbrName, id: item.groupId.toString() }))
            .filter((g, i, arr) => arr.findIndex(t => t.id === g.id) === i); // Distinct group

        if (this.groupList.length > 0 && this.departmentActive.length > 0) {
            this.groupList.unshift({ id: 'All', text: 'All' });
            if (this.menuPermission.list === 'Group'
                || this.menuPermission.list === 'Department'
                || this.menuPermission.list === 'Office'
                || this.menuPermission.list === 'Company'
                || this.menuPermission.list === 'All') {
                this.groupActive = [this.groupList[0]];
            } else {
                this.groupActive = [this.groupList.filter((grp) => grp.id === this.userLogged.groupId)[0]];
            }
        } else {
            this.group.setValue([]);
            this.groupActive = [];
        }

        this.getStaff(this.staffsInit);
    }

    getStaff(data: any) {
        if (this.groupActive.length > 0) {
            let groupSelected = [];
            if (this.groupActive.map(i => i.id).includes('All')) {
                groupSelected = this.groupList.map(i => i.id);
            } else {
                groupSelected = this.groupActive.map(i => i.id);
            }

            let deparmentSelected = [];
            if (this.departmentActive.length > 0) {
                if (this.departmentActive.map(i => i.id).includes('All')) {
                    deparmentSelected = this.departmentList.map(i => i.id);
                } else {
                    deparmentSelected = this.departmentActive.map(i => i.id);
                }
            }

            data = data.filter(f => f.userId !== null && f.groupId !== null && f.departmentId !== null
                && deparmentSelected.includes(f.departmentId.toString())
                && groupSelected.includes(f.groupId.toString()));
        } else {
            data = [];
        }

        this.staffList = data
            .map(item => ({ text: item.userName, id: item.userId }))
            .filter((g, i, arr) => arr.findIndex(t => t.id === g.id) === i); // Distinct user

        if (this.staffList.length > 0) {
            this.staffList.unshift({ id: 'All', text: 'All' });
            if (this.menuPermission.list === 'Owner') {
                this.staffActive = [this.staffList.filter(stf => stf.id === this.userLogged.id)[0]];
            } else {
                this.staffActive = [this.staffList[0]];
            }
        } else {
            this.staff.setValue([]);
            this.staffActive = [];
        }
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
        const body: ReportInterface.ISearchDataCriteria = {
            serviceDateFrom: this.dateType.value[0].id === "ServiceDate" ? formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            serviceDateTo: this.dateType.value[0].id === "ServiceDate" ? formatDate(this.serviceDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            createdDateFrom: this.dateType.value[0].id === "CreatedDate" ? formatDate(this.serviceDate.value.startDate, 'yyyy-MM-dd', 'en') : null,
            createdDateTo: this.dateType.value[0].id === "CreatedDate" ? formatDate(this.serviceDate.value.endDate, 'yyyy-MM-dd', 'en') : null,
            customerId: this.customer.value,
            service: this.mapObject(this.serviceActive, this.serviceList), // ---*
            currency: this.currencyActive[0].id, // ---**
            jobId: this.refNoType.value[0].id === "JOBID" ? this.refNo.value : null,
            mawb: this.refNoType.value[0].id === "MBL" ? this.refNo.value : null,
            hawb: this.refNoType.value[0].id === "HBL" ? this.refNo.value : null,
            officeId: this.mapObject(this.officeActive, this.officeList), // ---*
            departmentId: this.mapObject(this.departmentActive, this.departmentList), // ---*
            groupId: this.mapObject(this.groupActive, this.groupList), // ---*
            personInCharge: this.staffType.value[0].id === "PIC" ? this.mapObject(this.staffActive, this.staffList) : null, // this.staffActive.map((item) => item.id).toString().replace(/(?:,)/g, ';') : null,
            salesMan: this.staffType.value[0].id === "SALESMAN" ? this.mapObject(this.staffActive, this.staffList) : null, // this.staffActive.map((item) => item.id).toString().replace(/(?:,)/g, ';') : null,
            creator: this.staffType.value[0].id === "CREATOR" ? this.mapObject(this.staffActive, this.staffList) : null, // this.staffActive.map((item) => item.id).toString().replace(/(?:,)/g, ';') : null,
            carrierId: this.carrier.value,
            agentId: this.agent.value,
            pol: this.pol.value,
            pod: this.pod.value,
        };
        this.onSearch.emit(body);
    }

    mapObject(dataSelected: any[], dataList: any[]) {
        let result = '';
        if (dataSelected.length > 0) {
            if (dataSelected[0].id === 'All') {
                const list = dataList.filter(f => f.id !== 'All');
                result = list.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
            } else {
                result = dataSelected.map((item: any) => item.id).toString().replace(/(?:,)/g, ';');
            }
        }
        return result;
    }

    resetSearch() {
        this.formSearch.reset();
        this.resetFormControl(this.customer);
        this.resetFormControl(this.carrier);
        this.resetFormControl(this.agent);
        this.resetFormControl(this.pol);
        this.resetFormControl(this.pod);
        this.onSearch.emit(<any>{});

        this.dateTypeActive = [this.dateTypeList[0]];
        this.dateType.setValue(this.dateTypeActive);

        this.serviceActive = [this.serviceList[0]];
        this.service.setValue(this.serviceActive);

        this.currencyActive = [this.currencyList.filter((curr) => curr.id === "USD")[0]];
        this.currency.setValue(this.currencyActive);

        this.refNoTypeActive = [this.refNoTypeList[0]];
        this.refNoType.setValue(this.refNoTypeActive);

        this.staffTypeActive = [this.staffTypeList[0]];
        this.staffType.setValue(this.staffTypeActive);

        if (this.menuPermission.list !== 'None') {
            this.officeActive = [this.officeList.filter((off) => off.id === this.userLogged.officeId)[0]];
            this.office.setValue(this.officeActive);

            if (this.menuPermission.list === 'Office' || this.menuPermission.list === 'Company') {
                this.departmentActive = [this.departmentList[0]];
            } else {
                this.departmentActive = [this.departmentList.filter((dept) => dept.id === this.userLogged.departmentId)[0]];
            }
            this.department.setValue(this.departmentActive);

            if (this.menuPermission.list === 'Department') {
                this.groupActive = [this.groupList[0]];
            } else {
                this.groupActive = [this.groupList.filter((grp) => grp.id === this.userLogged.groupId)[0]];
            }
            this.group.setValue(this.groupActive);

            if (this.menuPermission.list === 'Group') {
                this.staffActive = [this.staffList[0]];
            } else {
                this.staffActive = [this.staffList.filter((stf) => stf.id === this.userLogged.id)[0]];
            }
            this.staff.setValue(this.staffActive);
        }

        this.serviceDate.setValue({
            startDate: this.createMoment().startOf('month').toDate(),
            endDate: this.createMoment().endOf('month').toDate(),
        });
    }

    getAllOffice() {
        this._systemRepo.getAllOffice()
            .pipe(
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe(
                (office: any) => {
                    if (!!office) {
                        office = office.map((item: any) => ({ companyId: item.buid, officeId: item.id, officeAbbrName: item.shortName }));
                        this.officesInit = office;
                        this.getOffice(office);
                    }
                },
            );
    }

    getAllDepartment() {
        this._systemRepo.getAllDepartment()
            .pipe(
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe(
                (department: any) => {
                    if (!!department) {
                        department = department.map((item: any) => ({ officeId: item.branchId, departmentId: item.id, departmentAbbrName: item.deptNameAbbr }));
                        department.forEach(element => {
                            this.groupSpecial.push({ departmentId: element.departmentId, groupId: 11, groupAbbrName: 'Manager' });
                        });
                        this.departmentsInit = department;
                        this.getDepartment(department);
                    }
                },
            );
    }

    getAllGroup() {
        this._systemRepo.getAllGroup()
            .pipe(
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe(
                (group: any) => {
                    if (!!group) {
                        group = group.map((item: any) => ({ departmentId: item.departmentId, groupId: item.id, groupAbbrName: item.shortName }));
                        group.forEach(element => {
                            this.groupSpecial.push({ departmentId: element.departmentId, groupId: element.groupId, groupAbbrName: element.groupAbbrName });
                        });
                        this.groupsInit = this.groupSpecial;
                        this.getGroup(this.groupSpecial);
                    }
                },
            );
    }

    getAllStaff() {
        this._systemRepo.getUserLevelByType({ type: 'All' })
            .pipe(
                catchError(this.catchError),
                finalize(() => { }),
            ).subscribe(
                (staff: any) => {
                    if (!!staff) {
                        this.staffsInit = staff;
                        this.getStaff(staff);
                    }
                },
            );
    }
}
