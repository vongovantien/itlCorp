import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { MenuResolveGuard } from "@core";
import { TruckingAssignmentComponent } from "./trucking-assignment/trucking-assignment.component";

const routes: Routes = [
    {
        path: "",
        redirectTo: "job-management",
    },
    {
        path: "job-management",
        loadChildren: () => import('./job-mangement/job-management.module').then(m => m.JobManagementModule),
        data: { name: 'Job Management', title: 'eFMS Ops Job', transactionType: null },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: "trucking-inland",
        loadChildren: () => import('./job-mangement/job-management.module').then(m => m.JobManagementModule),
        data: { name: 'Trucking Inland', title: 'eFMS Trucking Inland', transactionType: 'TKI' },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: "assigment",
        loadChildren: () => import('./assigment/assignment.module').then(m => m.AssignmentModule),
        data: { name: 'Assignment', title: 'eFMS Assignment' },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },

    {
        path: "custom-clearance",
        loadChildren: () => import('./custom-clearance/custom-clearance.module').then(m => m.CustomClearanceModule),
        data: { name: 'Custom Clearance', title: 'eFMS Custom Clearance' },
        resolve: {
            checkMenu: MenuResolveGuard
        },
    },
    {
        path: "trucking-assigment",
        component: TruckingAssignmentComponent,
        data: {
            name: "Trucking Assigment", title: 'eFMS Trucking'
        }, resolve: {
            checkMenu: MenuResolveGuard
        },
    }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: []
})
export class OperationRoutingModule { }
