<?php
require('fpdf.php');


$countPerPage = 7;
$classes = ['a', 'b', 'c', 'd'];
$studentsPerclass = 28;
$pagesPerClass = $studentsPerclass / $countPerPage;
$overallStudentCount = $studentsPerclass * count($classes) * 2;

$seed = 1235;
srand($seed);

function generateRandomString($length = 10) {
    $characters = '0123456789abcdefghijkmnpqrstuvwxyz';
    $charactersLength = strlen($characters);
    $randomString = '';
    for ($i = 0; $i < $length; $i++) {
        $randomString .= $characters[rand(0, $charactersLength - 1)];
    }
    return $randomString;
}


$randomStrings = [];
while(count($randomStrings) < $overallStudentCount) {
	$randString = generateRandomString(5);
	if(!in_array($randString, $randomStrings)) {
		$randomStrings[] = $randString;
	}
}

function generatePage($pdf, $level, $class) {
 	global $countPerPage, $randomStrings;

	$pdf->AddPage('P', 'A4');
	$levelSub = "Klasse " . $level . $class;

	$x1 = 10;
	$x2 = 210 / 2 + 10;
	$yHeight = 41;
	$y = 12.5;

	for ($i=0; $i < $countPerPage; $i++) { 
		$pdf->Rect($x1 - 2.5, $y - 2.5, 90, 31);
		$pdf->Rect($x2 - 2.5, $y - 2.5, 90, 31);

		$pdf->SetFont('Times', '', 14);
		$pdf->SetXY($x1, $y + 4);
		$pdf->Cell(0, 0, $levelSub);
		$pdf->SetXY($x2, $y + 4);
		$pdf->Cell(0, 0, $levelSub);

		$randomCode = $level . $class . '-' . array_pop($randomStrings);

		$pdf->SetFont('Times', 'B', 38);
		$pdf->SetXY($x1, $y + 19);
		$pdf->Cell(0, 0, $randomCode);
		$pdf->SetXY($x2, $y + 19);
		$pdf->Cell(0, 0, $randomCode);
		$y += $yHeight;
	}
}

$pdf = new FPDF();
$pdf->SetDrawColor(180);
$pdf->SetAutoPageBreak(false);

for ($i=4; $i <= 4; $i++) { 
	$classCount = count($classes);
	for ($c=0; $c < $classCount; $c++) { 
		for ($k=0; $k < $pagesPerClass; $k++) { 
			generatePage($pdf, $i, $classes[$c]);
		}
	}
}

$pdf->Output();
?>