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
import { CatalogueRepo, DocumentationRepo } from "src/app/shared/repositories";
import { takeUntil, catchError, finalize } from "rxjs/operators";
import { NgxSpinnerService } from "ngx-spinner";
import { ToastrService } from "ngx-toastr";
import { AppPage } from "src/app/app.base";

@Component({
    selector: "app-job-mangement-create",
    templateUrl: "./create-job.component.html",
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

    isDisplay: boolean = true;

    commodityGroups: any[] = [];

    constructor(
        private baseServices: BaseService,
        private api_menu: API_MENU,
        private router: Router,
        private spinner: NgxSpinnerService,
        private _toaster: ToastrService,
        private _catalogueRepo: CatalogueRepo,
        private _documentRepo: DocumentationRepo,
        private _router: Router
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
                active: true,
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
                active: true
            })
            .subscribe((res: any) => {
                this.listPort = res;
            });
    }

    private getListSupplier() {
        this.baseServices
            .post(this.api_menu.Catalogue.PartnerData.query, {
                partnerGroup: PartnerGroupEnum.CARRIER,
                active: true,
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
                active: true,
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
        this._catalogueRepo.getCommodityGroup({})
            .pipe()
            .subscribe(
                (res: any) => {
                    this.commodityGroups = res;
                    this.commodityGroups = dataHelper.prepareNg2SelectData(this.commodityGroups,
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
                    this._documentRepo.addOPSJob(this.OpsTransactionToAdd).pipe(
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

    gotoList() {
        this._router.navigate(["home/operation/job-management"]);

    }
}
