1251. Average Selling Price

SELECT
    P.product_id,
    ISNULL(ROUND(SUM(price * units) / SUM(1.0*units), 2),0) AS average_price	
FROM 
    prices P
LEFT JOIN 
    unitssold U
ON 
	P.product_id = U.product_id
WHERE 
    purchase_date BETWEEN start_date AND end_date OR purchase_date IS NULL
GROUP BY 
    P.product_id
	
------------------------------------------------------------------------------------	
1731. The Number of Employees Which Report to Each Employee

SELECT
    E2.employee_id,
    E2.name,
    COUNT(E1.name) AS reports_count,
    ROUND(AVG(CAST(E1.age AS DECIMAL)),0) AS average_age  
FROM 
    employees E1
JOIN 
    employees E2
ON 
	E1.reports_to = E2.employee_id 
GROUP BY 
    E2.employee_id, E2.name 
ORDER BY 
    E2.employee_id	
	

------------------------------------------------------------------------------------	
1084. Sales Analysis III

WITH CTE AS (
    SELECT DISTINCT product_id 
    FROM sales 
    WHERE sale_date < '2019-01-01' OR sale_date > '2019-03-31'
)

SELECT DISTINCT
    P.product_id,
    P.product_name
FROM 
    product P
JOIN 
    sales S ON P.product_id = S.product_id
WHERE 
    sale_date BETWEEN '2019-01-01' AND '2019-03-31'
    AND P.product_id NOT IN (SELECT product_id FROM CTE);
	
------------------------------------------------------------------------------------
1978. Employees Whose Manager Left the Company

SELECT
    employee_id
FROM
    employees
WHERE 
    salary < 30000 AND (
    manager_id NOT IN (SELECT employee_id FROM employees)
)
ORDER BY 
	employee_id ASC
	
------------------------------------------------------------------------------------
1141. User Activity for the Past 30 Days I

SELECT
    activity_date AS day,
    COUNT(DISTINCT user_id) AS active_users
FROM 
    activity
WHERE 
    activity_date >= DATEADD(day, -30, '2019-07-27')
GROUP BY
    activity_date;
	
------------------------------------------------------------------------------------
1633. Percentage of Users Attended a Contest

DECLARE @totalContester INT

WITH CTE AS (
    SELECT COUNT(DISTINCT user_id) AS TotalCount FROM users
)
SELECT @totalContester = TotalCount FROM CTE;

SELECT 
    contest_id,
    ROUND((100.0 * COUNT(R.user_id) / @totalContester), 2) AS percentage
FROM 
    users U
JOIN 
    register R ON U.user_id = R.user_id
GROUP BY 
    contest_id

------------------------------------------------------------------------------------
1873. Calculate Special Bonus

SELECT 
    employee_id,
    CASE WHEN employee_id%2=1 AND SUBSTRING(name,1,1) !='M' THEN salary ELSE 0 END AS bonus
FROM 
	employees 
	
------------------------------------------------------------------------------------	
1280. Students and Examinations	

WITH CTE AS (
    SELECT student_id, student_name, subject_name
    FROM students
    CROSS JOIN subjects
)

SELECT 
    CTE.student_id,
    CTE.student_name, 
    CTE.subject_name, 
    E.subject_name
FROM 
    CTE
LEFT JOIN 
    examinations E ON CTE.student_id = E.student_id

------------------------------------------------------------------------------------
-- 1211. Queries Quality and Percentage

-- SELECT
    -- query_name,
    -- ROUND(SUM(1.00*rating/position)/COUNT(query_name),2) AS quality,
    -- ROUND(100.0*SUM(CASE WHEN rating < 3 THEN 1 ELSE 0 END)/COUNT(query_name),2) AS poor_query_percentage
-- FROM 
    -- queries
-- GROUP BY query_name

-- 1517. Find Users With Valid E-Mails

-- SELECT 
    -- user_id, 
    -- name, 
    -- mail 
-- FROM 
    -- Users
-- WHERE 
    -- mail REGEXP '^[a-zA-Z][a-zA-Z0-9_.-]*@leetcode[.]com'
	
-- ------------------------------------------------------------------------------------	
-- 197. Rising Temperature

-- SELECT 
    -- w2.id
-- FROM 
    -- weather w1
-- JOIN 
    -- weather w2
-- ON DATEADD (day, 1, w1.recordDate) = w2.recordDate
-- WHERE 
    -- w2.temperature > w1.temperature

