import { Component } from "@angular/core";
import { BaseService } from "src/app/shared/services/base.service";
import { API_MENU } from "src/constants/api-menu.const";
import { OpsTransaction } from "src/app/shared/models/document/OpsTransaction.model";
import * as shipmentHelper from "src/helper/shipment.helper";
import * as dataHelper from "src/helper/data.helper";
import { PartnerGroupEnum } from "src/app/shared/enums/partnerGroup.enum";
import { NgForm } from "@angular/forms";
import { Router } from "@angular/router";
import { PlaceTypeEnum } from "src/app/shared/enums/placeType-enum";
import { CatalogueRepo, OperationRepo } from "src/app/shared/repositories";
import { takeUntil, catchError, finalize } from "rxjs/operators";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { AppPage } from "src/app/app.base";

@Component({
    selector: "app-job-mangement-create",
    templateUrl: "./create-job.component.html",
    styles:
        [`
            .form-add-job {
                margin-left: 15px;
            }
        `]
})
export class JobManagementCreateJobComponent extends AppPage {

    DataStorage: Object = null;
    productServices: any[] = [];
    serviceModes: any[] = [];
    shipmentModes: any[] = [];
    listCustomers: any[] = [];
    listSuppliers: any[] = [];
    listAgents: any[] = [];
    listPort: any[] = [];
    listBillingOps: any[] = [];
    OpsTransactionToAdd: OpsTransaction = new OpsTransaction();

    selectedDate: any;

    public items: Array<string> = [
        "option 1",
        "option 2",
        "option 3",
        "option 4",
        "option 5",
        "option 6",
        "option 7"
    ];

    private value: any = {};
    private _disabledV: string = "0";
    public disabled: boolean = false;

    isDisplay: boolean = true;

    commonityGroup: any[] = [];

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private router: Router,
        private _operationRepo: OperationRepo,
        private spinner: NgxSpinnerService,
        private _toaster: ToastrService,
        private _catalogueRepo: CatalogueRepo,
    ) {
        super();
        this.baseServices.dataStorage.subscribe(data => {
            this.DataStorage = data;
        });
    }

    ngOnInit() {
        this.getShipmentCommonData();
        this.getListCustomers();
        this.getListPorts();
        this.getListSupplier();
        this.getListAgent();
        this.getListBillingOps();
        this.getCommodityGroup();
        this.setCurrentBillingOPS();
    }
    setCurrentBillingOPS() {
        const claim = localStorage.getItem('id_token_claims_obj');
        const currenctUser = JSON.parse(claim)["id"];
        this.OpsTransactionToAdd.billingOpsId = currenctUser;
    }

    async getShipmentCommonData() {
        const data = await shipmentHelper.getOPSShipmentCommonData(
            this.baseServices,
            this.api_menu
        );
        this.productServices = dataHelper.prepareNg2SelectData(
            data.productServices,
            "value",
            "displayName"
        );
        this.serviceModes = dataHelper.prepareNg2SelectData(
            data.serviceModes,
            "value",
            "displayName"
        );
        this.shipmentModes = dataHelper.prepareNg2SelectData(
            data.shipmentModes,
            "value",
            "displayName"
        );
    }

    private getListCustomers() {
        this.baseServices
            .post(this.api_menu.Catalogue.PartnerData.query, {
                partnerGroup: PartnerGroupEnum.CUSTOMER,
                inactive: false,
                all: null
            })
            .subscribe((res: any) => {
                this.listCustomers = res;
            });
    }

    private getListPorts() {
        this.baseServices
            .post(this.api_menu.Catalogue.CatPlace.query, {
                placeType: PlaceTypeEnum.Port,
                inactive: false
            })
            .subscribe((res: any) => {
                this.listPort = res;
            });
    }

    private getListSupplier() {
        this.baseServices
            .post(this.api_menu.Catalogue.PartnerData.query, {
                partnerGroup: PartnerGroupEnum.CARRIER,
                inactive: false,
                all: null
            })
            .subscribe((res: any) => {
                this.listSuppliers = res;
            });
    }

    private getListAgent() {
        this.baseServices
            .post(this.api_menu.Catalogue.PartnerData.query, {
                partnerGroup: PartnerGroupEnum.AGENT,
                inactive: false,
                all: null
            })
            .subscribe((res: any) => {
                this.listAgents = res;
            });
    }

    private getListBillingOps() {
        this.baseServices
            .get(this.api_menu.System.User_Management.getAll)
            .subscribe((res: any) => {
                this.listBillingOps = res;
            });
    }

    getCommodityGroup() {
        this._catalogueRepo.getCommodityGroup()
            .pipe()
            .subscribe(
                (res: any) => {
                    this.commonityGroup = res;
                    this.commonityGroup = dataHelper.prepareNg2SelectData(this.commonityGroup,
                        "id",
                        "groupNameEn"
                    );
                }
            );
    }

    public submitNewOps(form: NgForm) {
        setTimeout(async () => {
            if (form.submitted) {
                const error = $("#add-new-ops-job-form").find("div.has-danger");
                if (error.length === 0) {
                    this.OpsTransactionToAdd.serviceDate = this.selectedDate.startDate != null ? dataHelper.dateTimeToUTC(this.selectedDate.startDate) : null;
                    this._operationRepo.addOPSJob(this.OpsTransactionToAdd).pipe(
                        takeUntil(this.ngUnsubscribe),
                        catchError(this.catchError),
                        finalize(() => { this.spinner.hide(); })
                    ).subscribe(
                        (res: any) => {
                            if (!res.status) {
                                this._toaster.error(res.message, '', { positionClass: 'toast-bottom-right' });
                            } else {
                                this.OpsTransactionToAdd = new OpsTransaction();
                                this.router.navigate([
                                    "/home/operation/job-edit/", res.data
                                ]);
                                this.resetDisplay();
                                form.onReset();
                                this._toaster.success(res.message, '', { positionClass: 'toast-bottom-right' });
                            }
                        }
                    );
                }
            }
        }, 300);
    }

    resetDisplay() {
        this.isDisplay = false;
        setTimeout(() => {
            this.isDisplay = true;
        }, 300);
    }

    private set disabledV(value: string) {
        this._disabledV = value;
        this.disabled = this._disabledV === "1";
    }

    public selected(value: any): void {
        console.log("Selected value is: ", value);
    }

    public removed(value: any): void {
        console.log("Removed value is: ", value);
    }

    public typed(value: any): void {
        console.log("New search input: ", value);
    }

    public refreshValue(value: any): void {
        this.value = value;
    }
}
