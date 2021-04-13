import {
  Component,
  Injector,
  ChangeDetectionStrategy,
  OnInit,
} from "@angular/core";
import { AppComponentBase } from "@shared/app-component-base";
import { appModuleAnimation } from "@shared/animations/routerTransition";
import {
  DashboardDto,
  DashboardServiceProxy,
} from "@shared/service-proxies/service-proxies";
@Component({
  templateUrl: "./home.component.html",
  animations: [appModuleAnimation()],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HomeComponent extends AppComponentBase implements OnInit {
  mostHousingDuePayersData: any;
  lessHousingDuePayersData: any;

  basicOptions: any;
  housingDueStatsOptions: any;

  barOptions: any;
  expensesData: any;
  housingDueStatsData: any;

  dashboardDto: DashboardDto;

  assetsClass: string;
  statsClass: string;

  constructor(
    injector: Injector,
    private _dashboardServiceProxy: DashboardServiceProxy
  ) {
    super(injector);
  }

  getClassValue(arrayLength: number): number {
    if (arrayLength < 4) {
      return arrayLength;
    } else {
      return arrayLength / 4;
    }
  }

  ngOnInit(): void {
    this._dashboardServiceProxy
      .getDashboardData()
      .subscribe((result: DashboardDto) => {
        this.dashboardDto = result;

        if (
          this.getClassValue(this.dashboardDto.paymentAccounts.length) === 1
        ) {
          this.assetsClass = "col-lg-12 col-12";
        } else if (
          this.getClassValue(this.dashboardDto.paymentAccounts.length) === 2
        ) {
          this.assetsClass = "col-lg-6 col-12";
        } else if (
          this.getClassValue(this.dashboardDto.paymentAccounts.length) === 3
        ) {
          this.assetsClass = "col-lg-4 col-12";
        } else if (
          this.getClassValue(this.dashboardDto.paymentAccounts.length) === 4
        ) {
          this.assetsClass = "col-lg-3 col-6";
        }

        if(this.dashboardDto.totalIncomeAmount > 0) {
          this.statsClass = "col-lg-4 col-12";
        } else {
          this.statsClass = "col-lg-6 col-12";
        }

        this.mostHousingDuePayersData = {
          labels: this.dashboardDto.mostHousingDuePayers.map(
            (p) => p.housingName
          ),
          datasets: [
            {
              label: this.l("MostHousingDuePayers"),
              backgroundColor: "#42A5F5",
              data: this.dashboardDto.mostHousingDuePayers.map(
                (p) => p.totalHousingDueAmount
              ),
            },
          ],
        };

        this.lessHousingDuePayersData = {
          labels: this.dashboardDto.lessHousingDuePayers.map(
            (p) => p.housingName
          ),
          datasets: [
            {
              label: this.l("LessHousingDuePayers"),
              backgroundColor: "#FFA726",
              data: this.dashboardDto.lessHousingDuePayers.map(
                (p) => p.totalHousingDueAmount
              ),
            },
          ],
        };

        this.expensesData = {
          labels: this.dashboardDto.expensesData.map(
            (p) => p.paymentCategoryName
          ),
          datasets: [
            {
              data: this.dashboardDto.expensesData.map((p) => p.totalAmount),
              backgroundColor: this.getRandomColorsArray(
                this.dashboardDto.expensesData.length
              ),
              // hoverBackgroundColor: ["#FF6384", "#36A2EB", "#FFCE56"],
            },
          ],
        };

        let totalHousingDueDefinition: any[] = [];
        totalHousingDueDefinition.push(
          this.dashboardDto.totalHousingDueStatsDto.totalHousingDueDefinition
        );

        let totalHousingDuePayment: any[] = [];
        totalHousingDuePayment.push(
          this.dashboardDto.totalHousingDueStatsDto.totalHousingDuePayment
        );

        this.housingDueStatsData = {
          datasets: [
            {
              label: this.l("TotalHousingDueDefinitions"),
              backgroundColor: "#42A5F5",
              data: totalHousingDueDefinition,
            },
            {
              label: this.l("TotalHousingDuePayments"),
              backgroundColor: "#FFA726",
              data: totalHousingDuePayment,
            },
          ],
        };
      });

    this.barOptions = {
      responsive: true,
      tooltips: {
        mode: "index",
        intersect: true,
      },
      scales: {
        yAxes: [
          {
            type: "linear",
            display: true,
            position: "left",
            id: "y-axis-1",
            ticks: { min: 0 },
          },
        ],
      },
    };

    this.housingDueStatsOptions = {
      responsive: true,
      tooltips: {
        mode: "index",
        intersect: true,
      },
      scales: {
        yAxes: [
          {
            type: "linear",
            display: true,
            position: "left",
            id: "y-axis-1",
            ticks: { min: 0 },
          },
        ],
      },
    };
  }

  getRandomColorsArray(arraySize: number): any[] {
    var colors = [];
    while (colors.length < arraySize) {
      do {
        var color = Math.floor(Math.random() * 1000000 + 1);
      } while (colors.indexOf(color) >= 0);
      colors.push("#" + ("000000" + color.toString(16)).slice(-6));
    }
    return colors;
  }
}
