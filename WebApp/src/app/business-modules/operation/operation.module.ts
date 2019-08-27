import { NgModule } from "@angular/core";
import { OperationRoutingModule } from "./operation-routing.module";
import { TruckingAssignmentComponent } from "./trucking-assignment/trucking-assignment.component";
import { SharedModule } from "src/app/shared/shared.module";

@NgModule({
    imports: [
        OperationRoutingModule,
        SharedModule
    ],
    declarations: [
        TruckingAssignmentComponent
    ]
})
export class OperationModule { }
